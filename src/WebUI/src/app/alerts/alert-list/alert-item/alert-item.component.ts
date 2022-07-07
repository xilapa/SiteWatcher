import {Component, Input, OnDestroy} from '@angular/core';
import {DetailedAlertView} from "../../common/alert";
import {DeviceService} from "../../../core/device/device.service";
import {Subscription} from "rxjs";

@Component({
    selector: 'sw-alert-item',
    templateUrl: './alert-item.component.html',
    styleUrls: ['./alert-item.component.css']
})
export class AlertItemComponent implements OnDestroy {

    @Input() alert: DetailedAlertView;
    isMobile: boolean;
    private deviceSub : Subscription;


    constructor(private deviceService : DeviceService) {
        this.deviceSub = deviceService.isMobileScreen().subscribe(mobile => this.isMobile = mobile);
    }

    ngOnDestroy(): void {
        this.deviceSub?.unsubscribe();
    }

}
