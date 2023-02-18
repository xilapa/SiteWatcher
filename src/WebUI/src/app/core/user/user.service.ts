import { HttpClient } from "@angular/common/http";
import { Injectable } from '@angular/core';
import { Router } from "@angular/router";
import { BehaviorSubject, Observable } from 'rxjs';
import { environment } from "../../../environments/environment";
import { Session } from "../auth/service/session";
import { UserRegister } from "../auth/user-register";
import { RegisterUserResult, UpdateUser, UpdateUserResult, User } from '../interfaces';
import { LocalStorageService } from "../local-storage/local-storage.service";
import { Data } from '../shared-data/shared-data';
import { ThemeService } from "../theme/theme.service";
import { TokenService } from '../token/token.service';

@Injectable({
    providedIn: 'root'
})
export class UserService {

    constructor(private readonly tokenService: TokenService,
                private readonly localStorage: LocalStorageService,
                private readonly httpClient: HttpClient,
                private readonly router: Router) {
        this.notifyUserLogged();
    }

    private readonly registerData = "register-data";
    private readonly profilePicKey = "profilePic";
    private readonly returnUrlKey = "returnUrl";
    private readonly emailConfirmedKey = "emailConfirmed";
    private readonly userKey = "userKey";
    private readonly baseRoute = "user";
    private userSubject = new BehaviorSubject<User | null>(null);

    public setUserData(session: Session): void {
        this.tokenService.setToken(session.SessionId);
        this.notifyUserLogged(session);
    }

    public getUser = (): Observable<User | null> =>
        this.userSubject;

    public getCurrentUser = (): User | null =>
        this.userSubject.getValue();

    public setUserRegisterData(registerData: Session) {
        // this.saveProfilePicUrl(registerData.ProfilePicUrl);
        // this.tokenService.setRegisterToken(registerData.Token);
        // const userRegister = jwt_decode(registerData.Token) as UserRegister;
        // userRegister.language = parseInt(userRegister.language as any) as ELanguage;
        // Data.Share(this.registerData, userRegister);
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

    public notifyUserLogged(session: Session | null = null): void {
        let user: User | null = null;
        if (session == null)
            user = this.localStorage.getItem(this.userKey);
            
        if (user == null && session != null) {
            user = {
                email: session.Email,
                emailConfirmed: session.EmailConfirmed,
                name: session.Name,
                profilePic: session.ProfilePicUrl,
                id: session.UserId,
                language: session.Language,
                theme: session.Theme
            } as User;
        }
        if (user == null) return;

        this.localStorage.setItem(this.userKey, JSON.stringify(user));

        this.userSubject.next(user);
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
        return this.httpClient.put<UpdateUserResult>(
            `${environment.baseApiUrl}/${this.baseRoute}`, updateData)
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

    emailConfirmed(): void {
        this.localStorage.setItem(this.emailConfirmedKey, 'true');
        this.notifyUserLogged();
    }

    reactivateAccount(token: string) : Observable<any> {
        return this.httpClient.put<any>(`${environment.baseApiUrl}/${this.baseRoute}/reactivate-account`, {token});
    }
}

