import {Injectable} from '@angular/core';
import {LocalStorageService} from "../local-storage/local-storage.service";

@Injectable({
  providedIn: 'root'
})
export class TokenService {

  private readonly tokenKey = "token";
  private readonly registerTokenKey = "TOKEN";

  constructor(private readonly localStorage: LocalStorageService) {
  }

  setToken(token: string): void {
    if(!token || token == 'null' || token == 'undefined')
      return;
    this.localStorage.setItem(this.tokenKey, token);
  }

  getToken = (): string | null =>
    this.localStorage.getItem(this.tokenKey);

  removeToken = (): void =>
    this.localStorage.removeItem(this.tokenKey);

  setRegisterToken = (token: string): void =>
    this.localStorage.setItem(this.registerTokenKey, token);

  getRegisterToken = (): string | null =>
    this.localStorage.getItem(this.registerTokenKey);

  removeRegisterToken = (): void =>
    this.localStorage.removeItem(this.registerTokenKey);
}
