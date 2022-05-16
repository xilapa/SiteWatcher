import {Component, OnInit} from '@angular/core';
import {DeviceService} from "../../core/device/device.service";
import {Observable} from "rxjs";

@Component({
    selector: 'sw-top-bar',
    templateUrl: './top-bar.component.html',
    styleUrls: ['./top-bar.component.css']
})
export class TopBarComponent implements OnInit {

    showTags = false;
    showSettings = false;
    mobileScreen$ :Observable<boolean>;
    searchText: string;
    searching = false;

    constructor(private readonly deviceService: DeviceService)
    { }

    ngOnInit(): void {
        this.mobileScreen$ = this.deviceService.isMobileScreen();
    }

    showTagsToggle(): void {
        this.showTags = !this.showTags;
        console.log(`show tags: ${this.showTags}`);
    }

    search(): void {
        this.searching = !this.searching;
    }

    showSettingsToggle(): void {
        this.showSettings = !this.showSettings;
        console.log(`show settings: ${this.showSettings}`);
    }

}
