import { HttpClient } from "@angular/common/http";
import { Injectable, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import jwt_decode from "jwt-decode";
import { BehaviorSubject, Observable, of, switchMap, tap } from 'rxjs';
import { environment } from "../../../environments/environment";
import { AuthenticationResult } from "../auth/service/authentication-result";
import { UserRegister } from "../auth/user-register";
import { RegisterUserResult, UpdateUser, UpdateUserResult, User } from '../interfaces';
import { ELanguage } from "../lang/language";
import { LocalStorageService } from "../local-storage/local-storage.service";
import { Data } from '../shared-data/shared-data';
import { ThemeService } from "../theme/theme.service";
import { TokenService } from '../token/token.service';

@Injectable({
    providedIn: 'root'
})
export class UserService implements OnInit  {

    constructor(private readonly tokenService: TokenService,
                private readonly localStorage: LocalStorageService,
                private readonly httpClient: HttpClient,
                private readonly router: Router) {
    }

    ngOnInit(): void {
        this.getUserInfoAndNotify().subscribe();
    }

    private readonly registerData = "register-data";
    private readonly profilePicKey = "profilePic";
    private readonly returnUrlKey = "returnUrl";
    private readonly emailConfirmedKey = "emailConfirmed";
    private readonly baseRoute = "user";
    private userSubject = new BehaviorSubject<User | null>(null);

    public setUserData(userData: AuthenticationResult): void {
        this.localStorage.removeItem(this.emailConfirmedKey);
        this.saveProfilePicUrl(userData.ProfilePicUrl);
        this.tokenService.setToken(userData.Token);
    }

    public setToken(token: string): void {
        this.localStorage.removeItem(this.emailConfirmedKey);
        this.tokenService.setToken(token);
    }

    public getUser(): Observable<User | null> {
        return this.userSubject.pipe(switchMap(u =>{
            if (u) return of(u);
            return this.getUserInfoAndNotify();
        }))
    }

    public getCurrentUser = (): User | null =>
        this.userSubject.getValue();

    public setUserRegisterData(registerData: AuthenticationResult) {
        this.saveProfilePicUrl(registerData.ProfilePicUrl);
        this.tokenService.setRegisterToken(registerData.Token);
        const userRegister = jwt_decode(registerData.Token) as UserRegister;
        userRegister.language = parseInt(userRegister.language as any) as ELanguage;
        Data.Share(this.registerData, userRegister);
    }

    private saveProfilePicUrl(picUrl: string | null): void {
        if (!picUrl) return;
        this.localStorage.setItem(this.profilePicKey, picUrl);
    }

    public removeUserRegisterData(): void {
        Data.RemoveByKeys(this.registerData);
        this.tokenService.removeRegisterToken();
    }

    public getUserRegisterData = (): UserRegister =>
        Data.Get(this.registerData) as UserRegister;

    public isLoggedIn = (): boolean =>
        !!this.tokenService.getToken();

    public hasRegisterData = (): boolean =>
        !!this.tokenService.getRegisterToken() && !!Data.Get(this.registerData);

    public getUserInfoAndNotify() : Observable<User | null>{
        const token = this.tokenService.getToken();
        if (!token || token == 'null' || token == 'undefined') return of(null)

        return this.httpClient.get<User>(`${environment.baseApiUrl}/${this.baseRoute}`)
            .pipe(tap(u => {
                if(!u) return

                u.ProfilePic = this.localStorage.getItem(this.profilePicKey);
                this.userSubject.next(u);
            }));
    }

    public logout(userDeletedAccount: boolean = false): void {
        userDeletedAccount && this.localStorage.removeItem(ThemeService.themeKey);
        this.clearLocalDataAndRedirect();
    }

    public logoutAllDevices(): void {
        this.httpClient.post(`${environment.baseApiUrl}/${this.baseRoute}/logout-all-devices`, {}).subscribe();
        this.clearLocalDataAndRedirect();
    }

    private clearLocalDataAndRedirect(): void {
        this.localStorage.removeItem(this.profilePicKey);
        this.tokenService.removeToken();
        this.userSubject.next(null);
        this.router.navigate(['/']);
    }

    public register(registerData: UserRegister): Observable<RegisterUserResult> {
        return this.httpClient.post<RegisterUserResult>(
            `${environment.baseApiUrl}/${this.baseRoute}/register`, registerData, {headers: {'authorization': `Bearer ${this.tokenService.getRegisterToken()}`}})
    }

    public update(updateData: UpdateUser): Observable<UpdateUserResult> {
        return this.httpClient.put<UpdateUserResult>(`${environment.baseApiUrl}/${this.baseRoute}`, updateData)
        .pipe(tap(res => {
            if (!res) return;
            this.userSubject.next(res.User);
        }))
    }

    public setReturnUrl(url: string): void {
        this.localStorage.setItem(this.returnUrlKey, url);
    }

    public redirecLoggedUser(): void {
        const returnUrl = this.localStorage.getAndRemove(this.returnUrlKey);

        if (returnUrl && returnUrl != 'null')
            this.router.navigateByUrl(returnUrl);
        else
            this.router.navigate(['dash']);
    }

    public deleteAccount(): Observable<any> {
        return this.httpClient.delete(`${environment.baseApiUrl}/${this.baseRoute}`);
    }

    public deactivateAccount(): Observable<any> {
        return this.httpClient.put(`${environment.baseApiUrl}/${this.baseRoute}/deactivate`, {});
    }

    public reactivateAccountEmail(userId: string): Observable<any> {
        return this.httpClient.put(`${environment.baseApiUrl}/${this.baseRoute}/send-reactivate-account-email`, {userId});
    }

    public resendConfirmationEmail(): Observable<any> {
        return this.httpClient.put(`${environment.baseApiUrl}/${this.baseRoute}/resend-confirmation-email`, {});
    }

    public confirmEmail(token: string): Observable<any> {
        return this.httpClient.put<any>(`${environment.baseApiUrl}/${this.baseRoute}/confirm-email`, {token});
    }

    emailConfirmed(): Observable<any> {
        this.localStorage.setItem(this.emailConfirmedKey, 'true');
        return this.getUserInfoAndNotify();
    }

    reactivateAccount(token: string) : Observable<any> {
        return this.httpClient.put<any>(`${environment.baseApiUrl}/${this.baseRoute}/reactivate-account`, {token});
    }
}

