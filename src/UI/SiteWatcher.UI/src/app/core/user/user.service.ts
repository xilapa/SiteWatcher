import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ELanguage } from '../enums/language';
import { UserRegister } from '../interfaces/user-register';
import { Data } from '../shared-data/shared-data';
import { TokenService } from '../token/token.service';
import jwt_decode from "jwt-decode";
import { User } from '../interfaces/user';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private readonly tokenService: TokenService) {
      const token = this.tokenService.getToken();
      if(token)
        this.decodeAndNotify(token);
   }

  private readonly registerData = "register-data";
  private userSubject = new BehaviorSubject<User | null>(null);

  public setToken(token : string) : void {
    // TODO: mover para método a ser executado após registro apenas
    Data.RemoveByKeys(this.registerData);
    this.tokenService.removeRegisterToken();
    // fim todo
    this.tokenService.setToken(token);
    this.decodeAndNotify(token);
  }
  
  public getUser = () : Observable<User | null> =>
    this.userSubject.asObservable();

  public setUserRegisterData(token : string){
    this.tokenService.setRegisterToken(token);
    const userRegister = jwt_decode(token) as UserRegister;
    userRegister.language = parseInt(userRegister.language as any) as ELanguage;
    Data.Share(this.registerData, userRegister);
  }

  public getUserRegisterData = () : UserRegister =>
    Data.Get(this.registerData) as UserRegister;

  public isLoggedIn = () : boolean  =>
    !!this.tokenService.getToken();

  public hasRegisterData = () : boolean =>
    !!this.tokenService.getRegisterToken() && !!Data.Get(this.registerData);
  
  private decodeAndNotify(token: string) : void {
    const user = jwt_decode(token) as User;
    user.language = parseInt(user.language as any) as ELanguage;
    user.emailConfirmed = JSON.parse((user as any)["email-confirmed"]);
    this.userSubject.next(user);
  }

  public logout() : void {
    this.tokenService.removeToken();
    this.userSubject.next(null);
  }

}

