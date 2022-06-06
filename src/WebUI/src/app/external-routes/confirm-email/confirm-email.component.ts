import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {UserService} from "../../core/user/user.service";
import {MessageService} from "primeng/api";
import {utils} from "../../core/utils/utils";
import {TranslocoService} from "@ngneat/transloco";

@Component({
    selector: 'sw-confirm-email',
    templateUrl: './confirm-email.component.html'
})
export class ConfirmEmailComponent implements OnInit {

    constructor(private readonly activatedRoute: ActivatedRoute,
                private readonly router: Router,
                private readonly userService: UserService,
                private readonly messageService: MessageService,
                private readonly translocoService: TranslocoService) {
    }

    ngOnInit(): void {
        const token: string | undefined = this.activatedRoute.snapshot.queryParams['t'];
        if (!token)
            this.router.navigate(['/']);

        this.userService.confirmEmail(token as string)
            .subscribe({
                next: () => {
                    this.userService.emailConfirmed();
                    utils.toastSuccess(this.messageService, this.translocoService,
                        this.translocoService.translate('settings.externalRoutes.confirmEmailSuccess'));
                    this.router.navigate(['/']);
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService,
                        this.translocoService)
                    this.router.navigateByUrl('/home');
                }
            })
    }
}
