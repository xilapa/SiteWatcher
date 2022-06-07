import { Component, OnInit } from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {UserService} from "../../core/user/user.service";
import {MessageService} from "primeng/api";
import {TranslocoService} from "@ngneat/transloco";
import {utils} from "../../core/utils/utils";
import {finalize} from "rxjs";

@Component({
  selector: 'sw-reactivate-account',
  templateUrl: './reactivate-account.component.html',
  styleUrls: ['./reactivate-account.component.css']
})
export class ReactivateAccountComponent implements OnInit {

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

        this.userService.reactivateAccount(token as string)
            .pipe(finalize(() => this.router.navigateByUrl('/home')))
            .subscribe({
                next: () => {
                    utils.toastSuccess(this.messageService, this.translocoService,
                        this.translocoService.translate('settings.externalRoutes.accountReactivated'));
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService,
                        this.translocoService);
                }
            })
    }

}
