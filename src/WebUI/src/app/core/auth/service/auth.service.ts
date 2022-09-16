import {DOCUMENT} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {Inject, Injectable} from '@angular/core';
import {Observable, tap} from 'rxjs';
import {environment} from 'src/environments/environment';
import { EAuthTask } from './auth-task';
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

    public googleLogin(): void {
        this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/login`;
    }

    public googleRegister(): void {
        this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/register`;
    }

    public authenticate(state: string, code: string, scope: string): Observable<AuthenticationResult> {
        return this.httpClient.post<AuthenticationResult>(
            `${environment.baseApiUrl}/${this.baseRoute}/authenticate`,
            {state, code, scope})
    }
}

