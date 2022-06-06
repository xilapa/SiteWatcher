import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {Data} from 'src/app/core/shared-data/shared-data';
import {ConfirmationService, MessageService} from 'primeng/api';
import {UserService} from 'src/app/core/user/user.service';
import {AuthService} from "../../core/auth/service/auth.service";
import {EAuthTask} from "../../core/auth/service/auth-task";
import {utils} from "../../core/utils/utils";
import {TranslocoService} from "@ngneat/transloco";
import {finalize, first} from "rxjs";

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
                private readonly confirmationService: ConfirmationService) {
    }

    ngOnInit(): void {
        const url = Data.GetAndRemove('authURL') as URL;
        if (url == null) {
            this.router.navigateByUrl('/home');
            return;
        }

        const state = url.searchParams.get('state') as string;
        const code = url.searchParams.get('code') as string;
        const scope = url.searchParams.get('scope') as string;

        this.authService.authenticate(state, code, scope)
            .subscribe({
                next: (response) => {

                    if (response.Result.Task == EAuthTask.Register) {
                        this.userService.setUserRegisterData(response.Result);
                        this.router.navigateByUrl('/home/register');
                    }

                    if (response.Result.Task == EAuthTask.Login) {
                        this.userService.setUserData(response.Result);
                        this.userService.redirecLoggedUser();
                    }

                    if (response.Result.Task == EAuthTask.Activate) {
                        utils.openModal(this.confirmationService, this.translocoService,
                            'home.auth.reactivateUserTitle', 'home.auth.reactivateUserMessage',
                            () => this.sendEmailToReactivateAccount(response.Result.Token), () => this.router.navigateByUrl('/home')
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
