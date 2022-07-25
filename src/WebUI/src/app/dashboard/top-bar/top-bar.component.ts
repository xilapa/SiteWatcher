import {Component, EventEmitter, OnInit, Output} from '@angular/core';
import {DeviceService} from "../../core/device/device.service";
import {Observable} from "rxjs";
import {UserService} from "../../core/user/user.service";
import {User} from "../../core/interfaces";
import {Router} from '@angular/router';
import {AlertService} from "../../alerts/service/alert.service";
import {utils} from "../../core/utils/utils";
import {MessageService} from "primeng/api";
import {TranslocoService} from "@ngneat/transloco";

@Component({
    selector: 'sw-top-bar',
    templateUrl: './top-bar.component.html',
    styleUrls: ['./top-bar.component.css']
})
export class TopBarComponent implements OnInit {

    showTags = false;
    mobileScreen$: Observable<boolean>;
    searchText: string;
    _searching = false;
    user$: Observable<User | null>
    profilePicError = false;
    @Output() searching = new EventEmitter<boolean>();

    constructor(private readonly deviceService: DeviceService,
                private readonly userService: UserService,
                private readonly router: Router,
                private readonly alertService: AlertService,
                private readonly messageService: MessageService,
                private readonly transloco: TranslocoService) {
    }

    ngOnInit(): void {
        this.mobileScreen$ = this.deviceService.isMobileScreen();
        this.user$ = this.userService.getUser();
        this.alertService.searchResults().subscribe(alerts => {
            if(!alerts)
                this.searchText ='';
        });
    }

    showTagsToggle(): void {
        this.showTags = !this.showTags;
        console.log(`show tags: ${this.showTags}`);
    }

    search(): void {
        this.alertService.prepareForSearch();
        this.router.navigateByUrl("/dash");
        this._searching = true;
        this.searching.next(this._searching);
        this.alertService.searchAlerts(this.searchText)
            .subscribe({
                next: _ => {
                    this._searching = false;
                    this.searching.next(this._searching);
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService,
                        this.transloco)
                }
            })

    }

    onProfilePicError(): void {
        this.profilePicError = true;
    }

    logout(): void {
        this.userService.logout();
    }
}
