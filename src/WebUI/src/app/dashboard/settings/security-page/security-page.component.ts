import { Component, OnDestroy, OnInit } from '@angular/core';
import { TranslocoService } from "@ngneat/transloco";
import { ConfirmationService, MessageService } from "primeng/api";
import { Observable, Subscription } from "rxjs";
import { DeviceService } from "../../../core/device/device.service";
import { UserService } from "../../../core/user/user.service";
import { utils } from "../../../core/utils/utils";

@Component({
    selector: 'sw-security-page',
    templateUrl: './security-page.component.html',
    styleUrls: ['./security-page.component.css']
})
export class SecurityPageComponent implements OnInit, OnDestroy {

    mobileScreen$: Observable<Boolean>;
    userSub: Subscription;
    userEmailConfirmed: boolean;

    constructor(private readonly deviceService: DeviceService,
                private readonly userService: UserService,
                private readonly confirmationService: ConfirmationService,
                private readonly translocoService: TranslocoService,
                private readonly messageService: MessageService) {
    }

    ngOnInit(): void {
        this.mobileScreen$ = this.deviceService.isMobileScreen();
        this.userSub = this.userService.getUser().subscribe(user => this.userEmailConfirmed = user?.EmailConfirmed as boolean);
    }

    ngOnDestroy() {
        this.userSub?.unsubscribe();
    }

    logoutAllDevices(): void {
        this.userService.logoutAllDevices();
    }

    resendConfirmationEmail(): void {
        this.userService.resendConfirmationEmail().subscribe({
            next: () => {
                utils.toastSuccess(this.messageService, this.translocoService, 'settings.security.confirmationEmailSend')
            },
            error: (errorResponse) => {
                utils.toastError(errorResponse, this.messageService,
                    this.translocoService)
            }
        });
    }

    deactivateAccount(): void {
        utils.openModal(this.confirmationService, this.translocoService,
            'settings.security.deactivateAccountTitle', 'settings.security.deactivateAccountConfirmation',
            () => this.userService.deactivateAccount()
                .subscribe({
                    next: () => {
                        utils.toastSuccess(this.messageService, this.translocoService,
                            'settings.security.deactivateAccountSuccess');
                        this.userService.logout(true);
                    },
                    error: (errorResponse) => {
                        utils.toastError(errorResponse, this.messageService,
                            this.translocoService)
                    }
                })
        );
    }

    deleteAccount(): void {
        utils.openModal(this.confirmationService, this.translocoService,
            'settings.security.deleteAccountTitle', 'settings.security.deleteAccountConfirmation',
            () => this.userService.deleteAccount()
                .subscribe({
                    next: () => {
                        utils.toastSuccess(this.messageService, this.translocoService,
                            'settings.security.deleteAccountSuccess');
                        this.userService.logout(true);
                    },
                    error: (errorResponse) => {
                        utils.toastError(errorResponse, this.messageService,
                            this.translocoService)
                    }
                })
        );
    }
}
