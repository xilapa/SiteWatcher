import {Component, Input, OnInit} from '@angular/core';
import {AlertService} from "../service/alert.service";
import {DetailedAlertView} from "../common/alert";

@Component({
    selector: 'sw-alert-list',
    templateUrl: './alert-list.component.html',
    styleUrls: ['./alert-list.component.css']
})
export class AlertListComponent implements OnInit {

    @Input() alerts : DetailedAlertView[];

    constructor(private readonly alertService: AlertService) {
    }

    ngOnInit(): void {
        this.alertService.getUserAlerts().subscribe(alerts => this.alerts = alerts);
    }

}
