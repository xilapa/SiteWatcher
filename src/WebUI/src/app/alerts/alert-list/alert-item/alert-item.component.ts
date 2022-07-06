import {Component, Input, OnInit} from '@angular/core';
import {DetailedAlertView} from "../../common/alert";
import {DeviceService} from "../../../core/device/device.service";
import {Observable} from "rxjs";
import {AlertUtils} from "../../common/alert-frequency";
import {WatchModeUtils} from "../../common/e-watch-mode";

@Component({
    selector: 'sw-alert-item',
    templateUrl: './alert-item.component.html',
    styleUrls: ['./alert-item.component.css']
})
export class AlertItemComponent implements OnInit {

    @Input() alert: DetailedAlertView;
    isMobile: boolean;
    frequencyTranslationKey : string;
    watchModeTranslationKey : string;

    constructor(private deviceService : DeviceService) {

        deviceService.isMobileScreen().subscribe(mobile => this.isMobile = mobile);
    }

    ngOnInit(): void {
        this.frequencyTranslationKey = AlertUtils.getFrequencyTranslationKey(this.alert.Frequency);
        this.watchModeTranslationKey = WatchModeUtils.getWatchModeTranslationKey(this.alert.WatchMode.WatchMode);
    }

}
