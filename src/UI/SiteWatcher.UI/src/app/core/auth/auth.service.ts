import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { AuthenticationResult, ApiResponse } from '../interfaces';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
 
  private readonly baseRoute = "google-auth";

  constructor(@Inject(DOCUMENT) private readonly document: Document,
    private readonly httpClient: HttpClient)
  { }

  public googleLogin(): void{
    this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/login`;
  }

  public googleRegister(): void{
    this.document.location.href = `${environment.baseApiUrl}/${this.baseRoute}/register`;
  }

  public authenticate(state: string, code: string, scope: string): Observable<ApiResponse<AuthenticationResult>> {
    return this.httpClient.post<ApiResponse<AuthenticationResult>>(
      `${environment.baseApiUrl}/${this.baseRoute}/authenticate`,
      { state, code, scope }, {responseType: 'json'})
  }

}