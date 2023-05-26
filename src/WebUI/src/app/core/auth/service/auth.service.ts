import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { nanoid } from 'nanoid/async';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { LocalStorageService } from '../../local-storage/local-storage.service';
import { utils } from '../../utils/utils';
import { AuthenticationResult } from "./authentication-result";

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    private readonly baseRoute = "auth";
    private readonly codeVerifierKey = "codeVerifier";

    constructor(
        @Inject(DOCUMENT) private readonly document: Document,
        private readonly httpClient: HttpClient,
        private readonly localStorage: LocalStorageService,
    ) {
    }

    public async googleLogin(): Promise<void> {
        // redirect to google login sending code challenge
        const codeChallenge = await this.generateCodeChallenge();
        this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/start/google?codeChallenge=${encodeURIComponent(codeChallenge)}`;
    }

    private async generateCodeChallenge(): Promise<string> {
        // generate and store code verifier
        const codeVerifier = await nanoid(128);
        this.localStorage.setItem(this.codeVerifierKey, codeVerifier);

        // generate code challenge
        // hash256 of code verifier
        const codeVerifierHashArr = await utils.sha256Hash(codeVerifier);
        // base64 encode the hash
        return window.btoa(String.fromCharCode(...codeVerifierHashArr));      
    }

    public exchangeCode(code: string): Observable<AuthenticationResult> {
        // get and remove code verifier
        const codeVerifier = this.localStorage.getItem(this.codeVerifierKey);
        this.localStorage.removeItem(this.codeVerifierKey);

        // send code verifier and code to server
        return this.httpClient.post<AuthenticationResult>(
            `${environment.baseApiUrl}/${this.baseRoute}/exchange-code`,
            { code, codeVerifier })
    }
}

