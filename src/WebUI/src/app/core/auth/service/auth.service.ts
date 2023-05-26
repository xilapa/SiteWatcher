import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthenticationResult } from "./authentication-result";

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    private readonly baseRoute = "auth";

    constructor(
        @Inject(DOCUMENT) private readonly document: Document,
        private readonly httpClient: HttpClient
    ) {
    }

    public googleLogin(): void {
        this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/start/google`;
    }

    public exchangeToken(code: string): Observable<AuthenticationResult> {
        return this.httpClient.post<AuthenticationResult>(
            `${environment.baseApiUrl}/${this.baseRoute}/exchange-code`,
            {code})
    }
}

