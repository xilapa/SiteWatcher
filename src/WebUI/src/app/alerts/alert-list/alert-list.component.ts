import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {AlertService} from "../service/alert.service";
import {DetailedAlertView} from "../common/alert";
import {DeviceService} from "../../core/device/device.service";
import {Subscription} from "rxjs";

@Component({
    selector: 'sw-alert-list',
    templateUrl: './alert-list.component.html',
    styleUrls: ['./alert-list.component.css']
})
export class AlertListComponent implements OnInit, OnDestroy {

    @Input() alerts : DetailedAlertView[];
    private isMobile: boolean;
    private deviceSub: Subscription;

    constructor(private readonly alertService: AlertService, deviceService: DeviceService) {
        this.deviceSub = deviceService.isMobileScreen().subscribe(mobile => this.isMobile = mobile);
    }

    ngOnInit(): void {
        this.alertService
            .getUserAlerts(this.isMobile)
            .subscribe(alerts => this.alerts = alerts);
    }

    ngOnDestroy(): void {
        this.deviceSub?.unsubscribe();
    }

    onScroll(): void {
        this.alertService
            .getMoreUserAlerts(this.isMobile)
            .subscribe(alerts => this.alerts = alerts);
    }

}
