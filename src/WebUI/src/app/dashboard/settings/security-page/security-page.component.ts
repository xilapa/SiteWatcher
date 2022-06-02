import {Component, OnInit} from '@angular/core';
import {DeviceService} from "../../../core/device/device.service";
import {Observable} from "rxjs";
import {UserService} from "../../../core/user/user.service";
import {User} from "../../../core/interfaces";
import {ConfirmationService, MessageService} from "primeng/api";
import {TranslocoService} from "@ngneat/transloco";
import {utils} from "../../../core/utils/utils";

@Component({
    selector: 'sw-security-page',
    templateUrl: './security-page.component.html',
    styleUrls: ['./security-page.component.css']
})
export class SecurityPageComponent implements OnInit {

    mobileScreen$: Observable<Boolean>;
    user$: Observable<User | null>;

    constructor(private readonly deviceService: DeviceService,
                private readonly userService: UserService,
                private readonly confirmationService: ConfirmationService,
                private readonly translocoService: TranslocoService,
                private readonly messageService: MessageService) {
    }

    ngOnInit(): void {
        this.mobileScreen$ = this.deviceService.isMobileScreen();
        this.user$ = this.userService.getUser();
    }

    logoutAllDevices(): void {
        this.userService.logoutAllDevices();
    }

    deactivateAccount() {
        utils.openModal(this.confirmationService, this.translocoService,
            'settings.security.deactivateAccountTitle','settings.security.deactivateAccountConfirmation',
            () => this.userService.deactivateAccount()
                .subscribe(() => {
                    utils.toastSuccess(this.messageService,this.translocoService,
                        'settings.security.deactivateAccountSuccess');
                    this.userService.logout(true);
                })
        );
    }

    deleteAccount() {
        utils.openModal(this.confirmationService, this.translocoService,
            'settings.security.deleteAccountTitle','settings.security.deleteAccountConfirmation',
            () => this.userService.deleteAccount()
                .subscribe(() => {
                    utils.toastSuccess(this.messageService,this.translocoService,
                        'settings.security.deleteAccountSuccess');
                    this.userService.logout(true);
                })
        );
    }
}
