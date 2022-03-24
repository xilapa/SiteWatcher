import { DOCUMENT } from '@angular/common';
import { Inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TokenService {

  private readonly localStorage : Storage;
  private readonly tokenKey = "token";
  private readonly registerTokenKey = "TOKEN";

  constructor(@Inject(DOCUMENT) doc: Document){
    this.localStorage = doc.defaultView?.localStorage as Storage;
  }

  setToken = (token: string) : void =>
    this.localStorage.setItem(this.tokenKey, token);
  
  getToken = (): string | null => 
    this.localStorage.getItem(this.tokenKey);

  removeToken = () : void =>
    this.localStorage.removeItem(this.tokenKey);

  setRegisterToken = (token: string) : void =>
    this.localStorage.setItem(this.registerTokenKey, token);
  
  getRegisterToken = () : string | null =>
    this.localStorage.getItem(this.registerTokenKey);

  removeRegisterToken = () : void =>
    this.localStorage.removeItem(this.registerTokenKey);
  
}
