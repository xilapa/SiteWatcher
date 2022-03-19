import { DOCUMENT } from '@angular/common';
import { Inject, Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TokenService {

  private readonly localStorage : Storage;
  private readonly tokenKey = "token";

  constructor(@Inject(DOCUMENT) doc: Document){
    this.localStorage = doc.defaultView?.localStorage as Storage;
  }

  setToken = (token: string) : void =>
    this.localStorage.setItem(this.tokenKey, token);
  
  getToken = (): string | null => 
    this.localStorage.getItem(this.tokenKey);
  
}
