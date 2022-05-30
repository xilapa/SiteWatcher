import {DOCUMENT} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {Inject, Injectable} from '@angular/core';
import {Observable} from 'rxjs';
import {environment} from 'src/environments/environment';
import {ApiResponse} from "../../interfaces";
import {AuthenticationResult} from "./authentication-result";

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    private readonly baseRoute = "google-auth";

    constructor(
        @Inject(DOCUMENT) private readonly document: Document,
        private readonly httpClient: HttpClient
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
}

