import {Injectable} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {Data} from '../shared-data/shared-data';
import {TokenService} from '../token/token.service';
import jwt_decode from "jwt-decode";
import {User} from '../interfaces';
import {UserRegister} from "../auth/user-register";
import {ELanguage} from "../lang/language";
import {AuthenticationResult} from "../auth/service/authentication-result";
import {LocalStorageService} from "../local-storage/local-storage.service";

@Injectable({
    providedIn: 'root'
})
export class UserService {

    constructor(private readonly tokenService: TokenService,
                private readonly localStorage : LocalStorageService) {
        const token = this.tokenService.getToken();
        if (token)
            this.decodeAndNotify(token);
    }

    private readonly registerData = "register-data";
    private readonly profilePicKey = "profilePic";
    private userSubject = new BehaviorSubject<User | null>(null);

    public setUserData(userData: AuthenticationResult) : void {
        this.saveProfilePicUrl(userData.ProfilePicUrl);
        this.tokenService.setToken(userData.Token);
        this.decodeAndNotify(userData.Token);
    }

    public setToken(token: string): void {
        this.tokenService.setToken(token);
        this.decodeAndNotify(token);
    }

    public getUser = (): Observable<User | null> =>
        this.userSubject.asObservable();

    public setUserRegisterData(registerData: AuthenticationResult) {
        this.saveProfilePicUrl(registerData.ProfilePicUrl);
        this.tokenService.setRegisterToken(registerData.Token);
        const userRegister = jwt_decode(registerData.Token) as UserRegister;
        userRegister.language = parseInt(userRegister.language as any) as ELanguage;
        Data.Share(this.registerData, userRegister);
    }

    private saveProfilePicUrl(picUrl : string | null) : void{
        if(!picUrl) return;
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

    private decodeAndNotify(token: string): void {
        const user = jwt_decode(token) as User;
        user.language = parseInt(user.language as any) as ELanguage;
        user.emailConfirmed = JSON.parse((user as any)["email-confirmed"]);
        user.profilePic = this.localStorage.getItem(this.profilePicKey);
        this.userSubject.next(user);
    }

    public logout(): void {
        this.tokenService.removeToken();
        this.userSubject.next(null);
    }
}

