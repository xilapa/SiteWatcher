import {Injectable} from '@angular/core';
import {
    HttpRequest,
    HttpHandler,
    HttpEvent,
    HttpInterceptor,
    HttpErrorResponse
} from '@angular/common/http';
import {Observable, tap} from 'rxjs';
import {Router} from '@angular/router';
import {UserService} from "../../user/user.service";

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {

    constructor(
        private readonly router: Router,
        private readonly userService: UserService
    ) {
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
