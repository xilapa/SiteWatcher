import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TranslocoService } from "@ngneat/transloco";
import { ConfirmationService, MessageService } from 'primeng/api';
import { finalize } from "rxjs";
import { UserService } from 'src/app/core/user/user.service';
import { EAuthTask } from "../../core/auth/service/auth-task";
import { AuthService } from "../../core/auth/service/auth.service";
import { utils } from "../../core/utils/utils";

@Component({
    selector: 'sw-auth',
    templateUrl: './auth.component.html'
})
export class AuthComponent implements OnInit {

    constructor(private readonly authService: AuthService,
                private readonly router: Router,
                private readonly messageService: MessageService,
                private readonly userService: UserService,
                private readonly translocoService: TranslocoService,
                private readonly confirmationService: ConfirmationService,
                @Inject(DOCUMENT) private readonly doc: Document) {
    }

    ngOnInit(): void {
        let routeHash = this.doc.location.hash
        if (routeHash.indexOf('#') > -1) routeHash = routeHash.replace('#', '')
        
        const url = new URL(this.doc.location.origin + routeHash)

        const token = url.searchParams.get('token') as string
        if (token == null){
            this.router.navigateByUrl('/home')
            return
        }
        this.authService.exchangeToken(token)
            .subscribe({
                next: (response) => {
                    if (!response) {
                        utils.toastError(null, this.messageService,
                            this.translocoService)
                        this.router.navigateByUrl('/home');
                    }
                    if (response.Task == EAuthTask.Register) {
                        this.userService.setUserRegisterData(response);
                        this.router.navigateByUrl('/home/register');
                    }

                    if (response.Task == EAuthTask.Login) {
                        this.userService.setUserData(response);
                        this.userService.redirecLoggedUser();
                    }

                    if (response.Task == EAuthTask.Activate) {
                        utils.openModal(this.confirmationService, this.translocoService,
                            'home.auth.reactivateUserTitle', 'home.auth.reactivateUserMessage',
                            () => this.sendEmailToReactivateAccount(response.Token), () => this.router.navigateByUrl('/home')
                        );
                    }
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService,
                        this.translocoService)
                    this.router.navigateByUrl('/home');
                }
            });
    }

    private sendEmailToReactivateAccount(userId: string): void {
        this.userService.reactivateAccountEmail(userId)
            .pipe(finalize(() => this.router.navigateByUrl('/home')))
            .subscribe({
            next: () => {
                utils.toastSuccess(this.messageService, this.translocoService,
                    this.translocoService.translate('home.auth.reactivateEmailSend'));
            },
            error: (errorResponse) => {
                utils.toastError(errorResponse, this.messageService,
                    this.translocoService)
            }
        });
    }
}
