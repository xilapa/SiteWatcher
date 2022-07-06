import {Component, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, Validators} from '@angular/forms';
import {Router} from "@angular/router";
import {DropdownOption} from "../../core/interfaces/dropdown-option";
import {AlertFrequency} from "../common/alert-frequency";
import {TranslocoService} from "@ngneat/transloco";
import { EWatchMode } from '../common/e-watch-mode';
import {finalize, Subscription} from "rxjs";
import {uriValidator} from "../../common/validators/uri.validator";
import {watchModeTermValidator} from "../../common/validators/watch-mode-term.validator";
import {CreateAlertModel} from "../common/alert";
import {AlertService} from "../service/alert.service";
import {utils} from "../../core/utils/utils";
import {MessageService} from "primeng/api";

@Component({
    selector: 'sw-create-update-alert',
    templateUrl: './create-update-alert.component.html',
    styleUrls: ['./create-update-alert.component.css']
})
export class CreateUpdateAlertComponent implements OnInit, OnDestroy {

    createUpdateAlertForm: FormGroup;
    inputFormName: AbstractControl | null;
    inputFormSiteName: AbstractControl | null;
    inputFormSiteUri: AbstractControl | null;
    inputFormWatchModeTerm: AbstractControl | null;
    frequencyOptions: DropdownOption<number>[];
    watchModeOptions: DropdownOption<number>[];
    termWatchModeSelected = false;
    pageTitleTranslationKey: string;
    doingRequest: boolean;
    private activePage: string;
    private watchModeSub : Subscription | undefined;

    constructor(private readonly router: Router,
                private readonly formBuilder: FormBuilder,
                private readonly transloco: TranslocoService,
                private readonly alertService: AlertService,
                private readonly messageService: MessageService) {
        this.frequencyOptions = [
            {Display: transloco.translate("alert.frequency.twoHours"), Value: AlertFrequency.TwoHours},
            {Display: transloco.translate("alert.frequency.fourHours"), Value: AlertFrequency.FourHours},
            {Display: transloco.translate("alert.frequency.eightHours"), Value: AlertFrequency.EightHours},
            {Display: transloco.translate("alert.frequency.twelveHours"), Value: AlertFrequency.TwelveHours},
            {Display: transloco.translate("alert.frequency.twentyFourHours"), Value: AlertFrequency.TwentyFourHours},
        ];

        this.watchModeOptions = [
            {Display: transloco.translate("alert.watchMode.anyChanges"), Value: EWatchMode.AnyChanges},
            {Display: transloco.translate("alert.watchMode.term"), Value: EWatchMode.Term}
        ]
    }

    ngOnDestroy(): void {
        this.watchModeSub?.unsubscribe();
    }

    ngOnInit(): void {
        const slashIndex = this.router.url.lastIndexOf('/');
        this.activePage = this.router.url.substring(slashIndex + 1);
        this.pageTitleTranslationKey = CreateUpdateAlertComponent.getTranslationKey(this.activePage);

        // TODO: define initial values when updating
        this.createUpdateAlertForm = this.formBuilder.group(
            {
                name: [, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                frequency: [AlertFrequency.TwoHours, [Validators.required]],
                siteName: [, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                siteUri: [, [Validators.required, uriValidator]],
                watchMode: [EWatchMode.AnyChanges, [Validators.required]],
                term: [, watchModeTermValidator]
            }
        );

        this.inputFormName = this.createUpdateAlertForm.get('name');
        this.inputFormSiteName = this.createUpdateAlertForm.get('siteName');
        this.inputFormSiteUri = this.createUpdateAlertForm.get('siteUri');
        this.inputFormWatchModeTerm = this.createUpdateAlertForm.get('term');

        this.watchModeSub = this.createUpdateAlertForm.get('watchMode')?.valueChanges
            .subscribe((watchMode: EWatchMode) => {
                    this.termWatchModeSelected = EWatchMode.Term == watchMode;
                    if (!this.termWatchModeSelected)
                        this.inputFormWatchModeTerm?.reset();
            });

    }

    private static getTranslationKey(pageName: string): string {
        return `alert.createUpdate.${pageName}Title`
    }

    public send(): void {
        this.doingRequest = true;
        if(this.activePage == 'create')
            this.createAlert();
        // TODO: condition to update method
    }

    private createAlert(): void {
        const alertData = this.createUpdateAlertForm.getRawValue() as CreateAlertModel;
        this.alertService.createAlert(alertData)
            .pipe(finalize(() => this.doingRequest = false))
            .subscribe({
                next: (response) => {
                    utils.toastSuccess(this.messageService, this.transloco,
                        this.transloco.translate('alert.createUpdate.created'));
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService,
                        this.transloco)
                }
            })
    }
}
