import {Component, OnInit} from '@angular/core';
import {DynamicDialogConfig, DynamicDialogRef} from "primeng/dynamicdialog";
import {DetailedAlertView} from "../common/alert";
import {EWatchMode} from "../common/e-watch-mode";

@Component({
    selector: 'sw-alert-details',
    templateUrl: './alert-details.component.html',
    styleUrls: ['./alert-details.component.css']
})
export class AlertDetailsComponent implements OnInit {

    alert: DetailedAlertView;
    watchModeEnum = EWatchMode;

    constructor(private readonly dialogConfig: DynamicDialogConfig,
                private readonly dialogRef: DynamicDialogRef) {
    }

    ngOnInit(): void {
        this.alert = this.dialogConfig.data.alert;
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
