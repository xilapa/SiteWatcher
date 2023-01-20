import { Component, OnDestroy, OnInit } from '@angular/core';
import { DialogService } from "primeng/dynamicdialog";
import { first, Subscription } from "rxjs";
import { DeviceService } from "../../core/device/device.service";
import { AlertDetailsComponent } from "../alert-details/alert-details.component";
import { DetailedAlertView } from "../common/alert";
import { AlertService } from "../service/alert.service";

@Component({
    templateUrl: './alert-list.component.html',
    styleUrls: ['./alert-list.component.css'],
    providers: [DialogService]
})
export class AlertListComponent implements OnInit, OnDestroy {

    alerts: DetailedAlertView[];
    private isMobile: boolean;
    private deviceSub: Subscription;
    private initialAlertsSub: Subscription;
    private scrollAlertsSub: Subscription;
    private searchResultsSub: Subscription;
    private clearSearchResultsSub: Subscription;
    private detailsCloseSub: Subscription;
    private searchStartedSub: Subscription;
    public isSearchResults = false;

    constructor(private readonly alertService: AlertService,
                private readonly dialogService: DialogService,
                deviceService: DeviceService
    ) {
        this.deviceSub = deviceService.isMobileScreen().subscribe(mobile => this.isMobile = mobile);
    }

    ngOnInit(): void {
        this.initialAlertsSub = this.alertService
            .getUserAlerts(this.isMobile)
            .subscribe(alerts => this.alerts = alerts);

        this.searchStartedSub = this.alertService
            .searchStarted()
            .subscribe(_ => {
                this.alerts = [];
                this.isSearchResults = true;
            })

        this.alertService
            .searchResults()
            .subscribe(alerts => {
                if (alerts) {
                    this.alerts = alerts;
                    this.isSearchResults = true;
                } else {
                    this.alertService
                        .getUserAlerts(this.isMobile)
                        .pipe(first())
                        .subscribe(alerts => {
                            this.alerts = alerts;
                            this.isSearchResults = false;
                        });
                }

            });
    }

    ngOnDestroy(): void {
        this.deviceSub?.unsubscribe();
        this.initialAlertsSub?.unsubscribe();
        this.searchResultsSub?.unsubscribe();
        this.scrollAlertsSub?.unsubscribe();
        this.clearSearchResultsSub?.unsubscribe();
        this.detailsCloseSub?.unsubscribe();
        this.searchStartedSub?.unsubscribe();
    }

    onScroll(): void {
        if (this.isSearchResults)
            return;

        this.scrollAlertsSub = this.alertService
            .getMoreUserAlerts(this.isMobile)
            .subscribe(alerts => this.alerts = alerts);
    }

    seeDetails(alert: DetailedAlertView) {
        const ref = this.dialogService.open(AlertDetailsComponent, {
            data: {alert: alert},
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
}
