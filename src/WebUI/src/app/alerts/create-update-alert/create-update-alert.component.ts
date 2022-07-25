import {AfterViewChecked, Component, Inject, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, Validators} from '@angular/forms';
import {ActivatedRoute, Router} from "@angular/router";
import {DropdownOption} from "../../core/interfaces/dropdown-option";
import {EAlertFrequency} from "../common/e-alert-frequency";
import {TranslocoService} from "@ngneat/transloco";
import {EWatchMode} from '../common/e-watch-mode';
import {finalize, Subscription} from "rxjs";
import {uriValidator} from "../../common/validators/uri.validator";
import {watchModeTermValidator} from "../../common/validators/watch-mode-term.validator";
import {AlertUtils, CreateUpdateAlertModel, DetailedAlertView} from "../common/alert";
import {AlertService} from "../service/alert.service";
import {utils} from "../../core/utils/utils";
import {MessageService} from "primeng/api";
import {DOCUMENT} from "@angular/common";

@Component({
    selector: 'sw-create-update-alert',
    templateUrl: './create-update-alert.component.html',
    styleUrls: ['./create-update-alert.component.css']
})
export class CreateUpdateAlertComponent implements OnInit, OnDestroy, AfterViewChecked {

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
    private watchModeSub: Subscription | undefined;

    // update
    alertInitialValues: DetailedAlertView | undefined;
    alertCurrentValues: DetailedAlertView | undefined;

    constructor(private readonly router: Router,
                private readonly route: ActivatedRoute,
                private readonly formBuilder: FormBuilder,
                private readonly transloco: TranslocoService,
                private readonly alertService: AlertService,
                private readonly messageService: MessageService,
                @Inject(DOCUMENT) private readonly doc: Document) {
        this.loadDropdownTranslations();
    }

    ngAfterViewChecked(): void {
        this.loadDropdownTranslations();
        this.removeBuggyPrimeNgHiddenElement();
    }

    ngOnDestroy(): void {
        this.watchModeSub?.unsubscribe();
    }

    private removeBuggyPrimeNgHiddenElement(){
        const hiddenElement = this.doc.querySelector(".p-hidden-accessible");
        hiddenElement?.remove()
    }

    ngOnInit(): void {
        this.pageTitleTranslationKey = this.getTranslationKey(this.router.url);

        if(this.activePage == 'update'){
            const alertId = this.route.snapshot.paramMap.get('alertId') as string;
            if(!alertId) this.router.navigateByUrl('/dash');

            this.alertInitialValues = this.alertService.getLoadedAlert(alertId);
            if(!this.alertInitialValues) this.router.navigateByUrl('/dash');
            if(this.alertInitialValues?.WatchMode.WatchMode == EWatchMode.Term)
                this.termWatchModeSelected = true;
        }

        if(this.activePage == 'create'){
            this.alertInitialValues = {
                Name: '',
                Frequency: EAlertFrequency.TwoHours,
                Site: {Name: '', Uri : ''},
                WatchMode: {WatchMode: EWatchMode.AnyChanges}
            }
        }

        this.alertCurrentValues = this.alertInitialValues;
        console.log(this.alertCurrentValues)

        this.createUpdateAlertForm = this.formBuilder.group(
            {
                name: [this.alertCurrentValues?.Name, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                frequency: [this.alertCurrentValues?.Frequency ?? EAlertFrequency.TwoHours, [Validators.required]],
                siteName: [this.alertCurrentValues?.Site.Name, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                siteUri: [this.alertCurrentValues?.Site.Uri, [Validators.required, uriValidator]],
                watchMode: [this.alertCurrentValues?.WatchMode.WatchMode ?? EWatchMode.AnyChanges, [Validators.required]],
                term: [this.alertCurrentValues?.WatchMode.Term, watchModeTermValidator]
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

    public send(): void {
        this.doingRequest = true;
        if (this.activePage == 'create')
            this.createAlert();
        if(this.activePage == 'update')
            this.updateAlert();
    }

    private createAlert(): void {
        const alertData = this.createUpdateAlertForm.getRawValue() as CreateUpdateAlertModel;
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

    private updateAlert() : void {
        const updatedAlertRawData = this.createUpdateAlertForm.getRawValue() as CreateUpdateAlertModel;
        const updateData = AlertUtils
            .GetUpdateAlertData(updatedAlertRawData, this.alertInitialValues as DetailedAlertView);
        this.alertService.updateAlert(updateData)
            .pipe(finalize(() => this.doingRequest = false))
            .subscribe({
                next: (response) => {
                    utils.toastSuccess(this.messageService, this.transloco,
                        this.transloco.translate('alert.createUpdate.updated'));
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService,
                        this.transloco)
                }
            })
    }

    private getTranslationKey(url: string): string {
        this.activePage = url.indexOf('create') > -1 ? 'create' : 'update';
        return `alert.createUpdate.${this.activePage}Title`
    }

    private loadDropdownTranslations(): void {
        this.frequencyOptions = [
            {Display: this.transloco.translate("alert.frequency.twoHours"), Value: EAlertFrequency.TwoHours},
            {Display: this.transloco.translate("alert.frequency.fourHours"), Value: EAlertFrequency.FourHours},
            {Display: this.transloco.translate("alert.frequency.eightHours"), Value: EAlertFrequency.EightHours},
            {Display: this.transloco.translate("alert.frequency.twelveHours"), Value: EAlertFrequency.TwelveHours},
            {
                Display: this.transloco.translate("alert.frequency.twentyFourHours"),
                Value: EAlertFrequency.TwentyFourHours
            },
        ];

        this.watchModeOptions = [
            {Display: this.transloco.translate("alert.watchMode.anyChanges"), Value: EWatchMode.AnyChanges},
            {Display: this.transloco.translate("alert.watchMode.term"), Value: EWatchMode.Term}
        ]
    }
}
