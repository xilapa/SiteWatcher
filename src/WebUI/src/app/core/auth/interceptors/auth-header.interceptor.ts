import {
  HttpEvent, HttpHandler, HttpInterceptor, HttpRequest
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { TokenService } from "../../token/token.service";

@Injectable()
export class AuthHeaderInterceptor implements HttpInterceptor {

  constructor(private tokenService: TokenService) { }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.tokenService.getToken();
    if (token)
      // request = request.clone({ setHeaders: { 'authorization': `Bearer ${token}`} })
      request = request.clone({ withCredentials: true })

    return next.handle(request);
  }
}
