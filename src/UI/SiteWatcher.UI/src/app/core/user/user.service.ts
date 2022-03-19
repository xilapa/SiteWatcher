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

  constructor(private readonly tokenService: TokenService) { }

  private userSubject = new BehaviorSubject<User | null>(null);

  public setToken(token : string) : void {
    this.tokenService.setToken(token);
    const user = jwt_decode(token) as User;
    user.language = parseInt(user.language as any) as ELanguage;
    this.userSubject.next(user);
  }
  
  public getUser = () : Observable<User | null> =>
    this.userSubject.asObservable();

  public setUserRegisterData(token : string){
    const userRegister = jwt_decode(token) as UserRegister;
    userRegister.language = parseInt(userRegister.language as any) as ELanguage;
    Data.Share("register-data", userRegister);
  }

  public getUserRegisterData = () =>
    Data.GetAndRemove("register-data") as UserRegister;
  
}

