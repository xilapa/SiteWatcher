import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Session } from "./session";

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

    public googleRegister(): void {
        this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/start/google`;
    }

    public authenticate(token: string): Observable<Session> {
        return this.httpClient.post<Session>(
            `${environment.baseApiUrl}/${this.baseRoute}/session`,
            {token}, { withCredentials: true })
    }    
}

