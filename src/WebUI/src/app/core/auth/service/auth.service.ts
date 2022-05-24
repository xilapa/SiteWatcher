import {DOCUMENT} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {Inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from 'src/environments/environment';
import {CookieService} from 'ngx-cookie-service';
import {Router} from '@angular/router';
import {TokenService} from "../../token/token.service";
import {ApiResponse} from "../../interfaces";
import {AuthenticationResult} from "./authentication-result";
import {UserRegister} from "../user-register";

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
        private readonly router: Router
    ) {
    }

    public googleLogin(returnUrl: string | null): void {
        this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/login?returnUrl=${returnUrl ?? ''}`;
    }

    public googleRegister(returnUrl: string | null): void {
        this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/register?returnUrl=${returnUrl ?? ''}`;
    }

    public authenticate(state: string, code: string, scope: string): Observable<ApiResponse<AuthenticationResult>> {
        return this.httpClient.post<ApiResponse<AuthenticationResult>>(
            `${environment.baseApiUrl}/${this.baseRoute}/authenticate`,
            {state, code, scope})
    }

    public register(registerData: UserRegister): Observable<ApiResponse<string>> {
        return this.httpClient.post<ApiResponse<string>>(
            `${environment.baseApiUrl}/user/register`, registerData, {headers: {'authorization': `Bearer ${this.tokenService.getRegisterToken()}`}})
    }

    public redirecLoggedUser(): void {
        var cookie = this.cookieService.get('returnUrl');

        if (cookie && cookie != 'undefined') {
            this.cookieService.delete('returnUrl');
            this.router.navigateByUrl(cookie);
        } else {
            this.router.navigate(['dash']);
        }
    }

    public logout() : void {
        this.httpClient.post(`${environment.baseApiUrl}/${this.baseRoute}/logout`, {}).subscribe();
    }
}

