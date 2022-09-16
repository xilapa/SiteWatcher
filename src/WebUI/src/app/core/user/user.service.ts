import {Injectable} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {Data} from '../shared-data/shared-data';
import {TokenService} from '../token/token.service';
import jwt_decode from "jwt-decode";
import {User, UpdateUser, UpdateUserResult, RegisterUserResult} from '../interfaces';
import {UserRegister} from "../auth/user-register";
import {ELanguage} from "../lang/language";
import {AuthenticationResult} from "../auth/service/authentication-result";
import {LocalStorageService} from "../local-storage/local-storage.service";
import {Router} from "@angular/router";
import {environment} from "../../../environments/environment";
import {HttpClient} from "@angular/common/http";
import {ETheme} from "../theme/theme";
import {ThemeService} from "../theme/theme.service";

@Injectable({
    providedIn: 'root'
})
export class UserService {

    constructor(private readonly tokenService: TokenService,
                private readonly localStorage: LocalStorageService,
                private readonly httpClient: HttpClient,
                private readonly router: Router) {
        this.decodeAndNotify();
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
        this.decodeAndNotify(userData.Token);
    }

    public setToken(token: string): void {
        this.localStorage.removeItem(this.emailConfirmedKey);
        this.tokenService.setToken(token);
        this.decodeAndNotify(token);
    }

    public getUser = (): Observable<User | null> =>
        this.userSubject;

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

    public decodeAndNotify(token: string | null = null): void {
        if (!token || token == 'null' || token == 'undefined')
            token = this.tokenService.getToken();

        if (!token || token == 'null' || token == 'undefined')
            return;

        const user = jwt_decode(token) as User;
        user.language = parseInt(user.language as any) as ELanguage;
        user.theme = parseInt(user.theme as any) as ETheme;
        user.profilePic = this.localStorage.getItem(this.profilePicKey);

        const emailConfirmedValue = this.localStorage.getItem(this.emailConfirmedKey);
        if (emailConfirmedValue)
            user.emailConfirmed = JSON.parse(emailConfirmedValue);
        else
            user.emailConfirmed = JSON.parse((user as any)["email-confirmed"]);

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
        this.decodeAndNotify();
    }

    reactivateAccount(token: string) : Observable<any> {
        return this.httpClient.put<any>(`${environment.baseApiUrl}/${this.baseRoute}/reactivate-account`, {token});
    }
}

