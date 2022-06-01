import {Injectable} from '@angular/core';
import {BehaviorSubject, Observable} from 'rxjs';
import {Data} from '../shared-data/shared-data';
import {TokenService} from '../token/token.service';
import jwt_decode from "jwt-decode";
import {ApiResponse, User, UpdateUser, UpdateUserResult} from '../interfaces';
import {UserRegister} from "../auth/user-register";
import {ELanguage} from "../lang/language";
import {AuthenticationResult} from "../auth/service/authentication-result";
import {LocalStorageService} from "../local-storage/local-storage.service";
import {Router} from "@angular/router";
import {environment} from "../../../environments/environment";
import {HttpClient} from "@angular/common/http";
import {ETheme} from "../theme/theme";

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private readonly tokenService: TokenService,
              private readonly localStorage: LocalStorageService,
              private readonly httpClient: HttpClient,
              private readonly router: Router) {
    const token = this.tokenService.getToken();
    if (token)
      this.decodeAndNotify(token);
  }

  private readonly registerData = "register-data";
  private readonly profilePicKey = "profilePic";
  private readonly returnUrlKey = "returnUrl";
  private readonly baseRoute = "user";
  private userSubject = new BehaviorSubject<User | null>(null);

  public setUserData(userData: AuthenticationResult): void {
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

  private decodeAndNotify(token: string): void {
    const user = jwt_decode(token) as User;
    user.language = parseInt(user.language as any) as ELanguage;
    user.emailConfirmed = JSON.parse((user as any)["email-confirmed"]);
    user.theme = parseInt(user.theme as any) as ETheme;
    user.profilePic = this.localStorage.getItem(this.profilePicKey);
    this.userSubject.next(user);
  }

  public logout(): void {
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

  public register(registerData: UserRegister): Observable<ApiResponse<string>> {
    return this.httpClient.post<ApiResponse<string>>(
      `${environment.baseApiUrl}/${this.baseRoute}/register`, registerData, {headers: {'authorization': `Bearer ${this.tokenService.getRegisterToken()}`}})
  }

  public update(updateData: UpdateUser): Observable<ApiResponse<UpdateUserResult>> {
    return this.httpClient.put<ApiResponse<UpdateUserResult>>(
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
}

