import {Component, OnInit} from '@angular/core';
import {DeviceService} from "../../core/device/device.service";
import {Observable} from "rxjs";
import {UserService} from "../../core/user/user.service";
import {User} from "../../core/interfaces";
import { Router } from '@angular/router';

@Component({
    selector: 'sw-top-bar',
    templateUrl: './top-bar.component.html',
    styleUrls: ['./top-bar.component.css']
})
export class TopBarComponent implements OnInit {

    showTags = false;
    mobileScreen$ :Observable<boolean>;
    searchText: string;
    searching = false;
    user$: Observable<User | null>
    profilePicError = false;

    constructor(private readonly deviceService: DeviceService,
                private readonly userService: UserService,
                private readonly router: Router) {
    }

    ngOnInit(): void {
        this.mobileScreen$ = this.deviceService.isMobileScreen();
        this.user$ = this.userService.getUser();
    }

    showTagsToggle(): void {
        this.showTags = !this.showTags;
        console.log(`show tags: ${this.showTags}`);
    }

    search(): void {
        this.searching = !this.searching;
    }

    onProfilePicError(): void {
        this.profilePicError = true;
    }

    logout(): void {
        this.userService.logout();
    }
}
