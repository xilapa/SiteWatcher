import {Component, Input, OnDestroy, OnInit} from '@angular/core';
import {AlertService} from "../service/alert.service";
import {DetailedAlertView} from "../common/alert";
import {DeviceService} from "../../core/device/device.service";
import {Subscription} from "rxjs";
import {DialogService} from "primeng/dynamicdialog";
import {AlertDetailsComponent} from "../alert-details/alert-details.component";

@Component({
    selector: 'sw-alert-list',
    templateUrl: './alert-list.component.html',
    styleUrls: ['./alert-list.component.css'],
    providers: [DialogService]
})
export class AlertListComponent implements OnInit, OnDestroy {

    @Input() alerts : DetailedAlertView[];
    private isMobile: boolean;
    private deviceSub: Subscription;
    private initialAlertsSub: Subscription;
    private scrollAlertsSub: Subscription;
    private searchResultsSub: Subscription;
    private clearSearchResultsSub: Subscription;
    private detailsCloseSub: Subscription;
    isSearchResult = false;

    constructor(private readonly alertService: AlertService,
                private readonly dialogService: DialogService,
                deviceService: DeviceService
    ) {
        this.deviceSub = deviceService.isMobileScreen().subscribe(mobile => this.isMobile = mobile);
    }

    ngOnInit(): void {
        this.initialAlertsSub = this.alertService
            .getUserAlerts(this.isMobile)
            .subscribe(alerts => {
                this.alerts = alerts;
                this.isSearchResult = false;
            });

        this.searchResultsSub = this.alertService
            .searchResults()
            .subscribe(alerts => {
                this.alerts = alerts;
                this.isSearchResult = true;
            });
    }

    ngOnDestroy(): void {
        this.deviceSub?.unsubscribe();
        this.initialAlertsSub?.unsubscribe();
        this.searchResultsSub?.unsubscribe();
        this.scrollAlertsSub?.unsubscribe();
        this.clearSearchResultsSub?.unsubscribe();
        this.detailsCloseSub?.unsubscribe();
    }

    onScroll(): void {
        this.scrollAlertsSub = this.alertService
            .getMoreUserAlerts(this.isMobile)
            .subscribe(alerts => this.alerts = alerts);
    }

    seeDetails(alert: DetailedAlertView){
        const ref = this.dialogService.open(AlertDetailsComponent, {
            data:{ alert: alert },
            showHeader: false,
            width: '90%',
            dismissableMask: true,
            style: {"max-width": "590px", "padding": 0},
        });

        this.detailsCloseSub = ref.onClose.subscribe((deleted: boolean) => {
            if (deleted) {
                this.alertService
                    .getUserAlerts(this.isMobile)
                    .subscribe(alerts => this.alerts = alerts);
            }
        });
    }

    clear(){
        this.clearSearchResultsSub = this.alertService
            .loadTimelineAlerts(this.isMobile)
            .subscribe(alerts => {
                this.alerts = alerts;
                this.isSearchResult = false;
            });
    }

}
