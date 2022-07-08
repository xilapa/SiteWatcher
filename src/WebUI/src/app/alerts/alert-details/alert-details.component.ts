import {Component, OnInit} from '@angular/core';
import {DynamicDialogConfig, DynamicDialogRef} from "primeng/dynamicdialog";
import {DetailedAlertView} from "../common/alert";
import {EWatchMode} from "../common/e-watch-mode";
import {AlertService} from "../service/alert.service";

@Component({
    selector: 'sw-alert-details',
    templateUrl: './alert-details.component.html',
    styleUrls: ['./alert-details.component.css']
})
export class AlertDetailsComponent implements OnInit {

    alert: DetailedAlertView;
    watchModeEnum = EWatchMode;

    constructor(private readonly dialogConfig: DynamicDialogConfig,
                private readonly dialogRef: DynamicDialogRef,
                private readonly alertService: AlertService) {
    }

    ngOnInit(): void {
        this.alert = this.dialogConfig.data.alert;
        if(!this.alert.FullyLoaded)
            this.alertService.getAlertDetails(this.alert)
                .subscribe(alert => this.alert = alert);
    }

    close(): void {
        this.dialogRef.close();
    }

    deleteAlert() : void {
        console.log("delete");
        // todo
    }

    goToEditPage() : void {
        console.log("go to edit");
        // todo
    }

}
