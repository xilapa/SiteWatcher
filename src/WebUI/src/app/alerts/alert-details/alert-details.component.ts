import {Component, OnInit} from '@angular/core';
import {DynamicDialogConfig, DynamicDialogRef} from "primeng/dynamicdialog";
import {DetailedAlertView} from "../common/alert";
import {EWatchMode} from "../common/e-watch-mode";
import {AlertService} from "../service/alert.service";
import {utils} from "../../core/utils/utils";
import {MessageService} from "primeng/api";
import {TranslocoService} from "@ngneat/transloco";
import {Router} from "@angular/router";

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
                private readonly alertService: AlertService,
                private readonly messageService: MessageService,
                private readonly transloco: TranslocoService,
                private readonly router: Router) {
    }

    ngOnInit(): void {
        this.alert = this.dialogConfig.data.alert;
        if(!this.alert.FullyLoaded)
            this.alertService.getAlertDetails(this.alert)
                .subscribe(alert => this.alert = alert);
    }

    close(): void {
        this.dialogRef.close(false);
    }

    deleteAlert() : void {
        this.alertService.deleteAlert(this.alert.Id as string)
            .subscribe({
                next: (response) => {
                    this.dialogRef.close(true);
                    utils.toastSuccess(this.messageService, this.transloco,
                        this.transloco.translate('alert.createUpdate.created'));
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService,
                        this.transloco)
                }
            })
    }

    goToEditPage() : void {
        if(!this.alert.FullyLoaded)
            return;
        this.router.navigate(['dash/alert/update', this.alert.Id]);
        this.dialogRef.close(false);
    }

}
