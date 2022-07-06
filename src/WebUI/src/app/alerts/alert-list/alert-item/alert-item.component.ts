import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {DetailedAlertView} from "../../common/alert";
import {DeviceService} from "../../../core/device/device.service";
import {AlertUtils} from "../../common/alert-frequency";
import {WatchModeUtils} from "../../common/e-watch-mode";
import {TranslocoService} from "@ngneat/transloco";
import {LangUtils} from "../../../core/lang/lang.utils";
import {Subscription} from "rxjs";

@Component({
    selector: 'sw-alert-item',
    templateUrl: './alert-item.component.html',
    styleUrls: ['./alert-item.component.css']
})
export class AlertItemComponent implements OnInit, OnDestroy {

    @Input() alert: DetailedAlertView;
    isMobile: boolean;
    frequencyTranslationKey : string;
    watchModeTranslationKey : string;
    private deviceSub : Subscription;
    private readonly currentLocale: string;
    dateStringLocalized: string;

    constructor(private deviceService : DeviceService,
                private readonly transloco: TranslocoService,
                private window : Window) {
        this.deviceSub = deviceService.isMobileScreen().subscribe(mobile => this.isMobile = mobile);
        this.currentLocale = LangUtils.getCurrentLocale(window);
    }

    ngOnInit(): void {
        this.frequencyTranslationKey = AlertUtils.getFrequencyTranslationKey(this.alert.Frequency);
        this.watchModeTranslationKey = WatchModeUtils.getWatchModeTranslationKey(this.alert.WatchMode.WatchMode);
        this.dateStringLocalized = this.alert.CreatedAt.toLocaleDateString(this.currentLocale) + ' '
            + this.alert.CreatedAt.toLocaleTimeString(this.currentLocale);
    }

    ngOnDestroy(): void {
        this.deviceSub?.unsubscribe();
    }

}
