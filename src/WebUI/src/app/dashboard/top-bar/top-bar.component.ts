import {Component, OnInit} from '@angular/core';
import {DeviceService} from "../../core/device/device.service";
import {Observable, Subscription} from "rxjs";

@Component({
    selector: 'sw-top-bar',
    templateUrl: './top-bar.component.html',
    styleUrls: ['./top-bar.component.css']
})
export class TopBarComponent implements OnInit {

    showTags = false;
    mobileScreen$ :Observable<boolean>;

    constructor(private readonly deviceService: DeviceService)
    { }

    ngOnInit(): void {
        this.mobileScreen$ = this.deviceService.isMobileScreen();
    }

    showTagsToggle(): void {
        this.showTags = !this.showTags;
        console.log(`show tags: ${this.showTags}`);
    }

}
