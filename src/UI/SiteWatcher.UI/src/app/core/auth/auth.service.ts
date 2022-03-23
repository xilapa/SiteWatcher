import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthenticationResult, ApiResponse, UserRegister } from '../interfaces';
import { TokenService } from '../token/token.service';
import { CookieService } from 'ngx-cookie-service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
 
  private readonly baseRoute = "google-auth";

  constructor(
    @Inject(DOCUMENT) private readonly document: Document,
    private readonly httpClient: HttpClient,
    private readonly tokenService: TokenService,
    private readonly cookieService: CookieService,
    private readonly router : Router
    )
  { }

  public googleLogin(returnUrl: string): void{
    this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/login?returnUrl=${returnUrl}`;
  }

  public googleRegister(returnUrl: string): void{
    this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/register?returnUrl=${returnUrl}`;
  }

  public authenticate(state: string, code: string, scope: string): Observable<ApiResponse<AuthenticationResult>> {
    return this.httpClient.post<ApiResponse<AuthenticationResult>>(
      `${environment.baseApiUrl}/${this.baseRoute}/authenticate`,
      { state, code, scope }, {responseType: 'json'})
  }

  public register(registerData : UserRegister): Observable<ApiResponse<string>> {
    return this.httpClient.post<ApiResponse<string>>(
      `${environment.baseApiUrl}/user/register`, registerData, {responseType: 'json', headers: {'authorization': `Bearer ${this.tokenService.getRegisterToken()}`}})
  }

  public redirecLoggedUser() : void {
    var cookie = this.cookieService.get('returnUrl');

    if(cookie) {
      this.cookieService.delete('returnUrl');
      this.router.navigateByUrl(cookie);
    } 
    else {
      this.router.navigate(['dash']);
    }        
  }

}