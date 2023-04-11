import {
    HttpErrorResponse,
    HttpEvent,
    HttpHandler,
    HttpInterceptor,
    HttpRequest
} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { UserService } from "../../user/user.service";

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {

    constructor(private readonly userService: UserService) {
    }

    intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
        return next.handle(request)
            .pipe(
                tap({
                    error: err => {
                        if (this.userService.isLoggedIn() && err instanceof HttpErrorResponse && (err.status == 401 || err.status == 403)) {
                            this.userService.logout();
                        }
                    }
                }));
    }
}
