import { DOCUMENT } from "@angular/common";
import { AfterViewChecked, Component, Inject, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from "@angular/router";
import { TranslocoService } from "@ngneat/transloco";
import { MessageService } from "primeng/api";
import { Subscription, finalize } from "rxjs";
import { regexRuleValidator } from "src/app/common/validators/rule-type-regex.validator";
import { termRuleValidator } from "../../common/validators/rule-type-term.validator";
import { uriValidator } from "../../common/validators/uri.validator";
import { DropdownOption } from "../../core/interfaces/dropdown-option";
import { utils } from "../../core/utils/utils";
import { AlertUtils, CreateUpdateAlertModel, DetailedAlertView } from "../common/alert";
import { EAlertFrequency } from "../common/e-alert-frequency";
import { Rules } from '../common/e-rule-type';
import { AlertService } from "../service/alert.service";

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
    inputFormTermRule: AbstractControl | null;
    inputFormRegexRulePattern: AbstractControl | null;
    inputFormRegexRuleNotifyOnDisappearance: AbstractControl | null;
    frequencyOptions: DropdownOption<number>[];
    ruleOptions: DropdownOption<number>[];
    notifyOnDisappearanceOptions: DropdownOption<boolean>[];
    termRuleSelected = false;
    regexRuleSelected = false;
    anyChangesSelected = false;
    pageTitleTranslationKey: string;
    doingRequest: boolean;
    dataChanged = false;
    private activePage: string;
    private ruleSub: Subscription | undefined;
    private createUpdateAlertFormSub: Subscription;

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
        this.ruleSub?.unsubscribe();
        this.createUpdateAlertFormSub?.unsubscribe();
    }

    private removeBuggyPrimeNgHiddenElement() {
        const hiddenElement = this.doc.querySelector(".p-hidden-accessible");
        hiddenElement?.remove()
    }

    ngOnInit(): void {
        this.pageTitleTranslationKey = this.getTranslationKey(this.router.url);

        if (this.activePage == 'update') {
            const alertId = this.route.snapshot.paramMap.get('alertId') as string;
            if (!alertId) this.router.navigateByUrl('/dash');

            this.alertInitialValues = this.alertService.getLoadedAlert(alertId);
            if (!this.alertInitialValues) this.router.navigateByUrl('/dash');

            if (this.alertInitialValues?.Rule.Rule == Rules.Term)
                this.termRuleSelected = true;

            if (this.alertInitialValues?.Rule.Rule == Rules.Regex)
                this.regexRuleSelected = true;

            if (this.alertInitialValues?.Rule.Rule == Rules.AnyChanges)
                this.anyChangesSelected = true;
        }

        if (this.activePage == 'create') {
            this.alertInitialValues = {
                Name: '',
                Frequency: EAlertFrequency.TwoHours,
                Site: { Name: '', Uri: '' },
                Rule: { Rule: Rules.AnyChanges, NotifyOnDisappearance: true }
            }
            this.anyChangesSelected = true;
        }

        this.alertCurrentValues = this.alertInitialValues;

        this.createUpdateAlertForm = this.formBuilder.group(
            {
                name: [this.alertCurrentValues?.Name, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                frequency: [this.alertCurrentValues?.Frequency ?? EAlertFrequency.TwoHours, [Validators.required]],
                siteName: [this.alertCurrentValues?.Site.Name, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                siteUri: [this.alertCurrentValues?.Site.Uri, [Validators.required, uriValidator]],
                rule: [this.alertCurrentValues?.Rule.Rule ?? Rules.AnyChanges, [Validators.required]],
                term: [this.alertCurrentValues?.Rule.Term, termRuleValidator],
                regexPattern: [this.alertCurrentValues?.Rule.RegexPattern, regexRuleValidator],
                notifyOnDisappearance: [this.alertCurrentValues?.Rule.NotifyOnDisappearance ?? true]
            }
        );

        this.inputFormName = this.createUpdateAlertForm.get('name');
        this.inputFormSiteName = this.createUpdateAlertForm.get('siteName');
        this.inputFormSiteUri = this.createUpdateAlertForm.get('siteUri');
        this.inputFormTermRule = this.createUpdateAlertForm.get('term');
        this.inputFormRegexRulePattern = this.createUpdateAlertForm.get('regexPattern');
        this.inputFormRegexRuleNotifyOnDisappearance = this.createUpdateAlertForm.get('notifyOnDisappearance');

        this.ruleSub = this.createUpdateAlertForm.get('rule')?.valueChanges
            .subscribe((rule: Rules) => {
                this.termRuleSelected = Rules.Term == rule;
                if (!this.termRuleSelected)
                    this.inputFormTermRule?.reset();

                this.regexRuleSelected = Rules.Regex == rule;
                if (!this.regexRuleSelected) {
                    this.inputFormRegexRulePattern?.reset();
                    this.inputFormRegexRuleNotifyOnDisappearance?.reset();
                }else{
                    this.inputFormRegexRuleNotifyOnDisappearance?.setValue(true)
                }

                this.anyChangesSelected = Rules.AnyChanges == rule;
            });

        this.createUpdateAlertFormSub = this.createUpdateAlertForm.valueChanges.subscribe(() => this.checkIfDataChanged());

    }

    public send(): void {
        this.doingRequest = true;
        if (this.activePage == 'create')
            this.createAlert();
        if (this.activePage == 'update')
            this.updateAlert();
    }

    private createAlert(): void {
        const alertData = this.createUpdateAlertForm.getRawValue() as CreateUpdateAlertModel;
        this.alertService.createAlert(alertData)
            .pipe(finalize(() => this.doingRequest = false))
            .subscribe({
                next: (_) => {
                    utils.toastSuccess(this.messageService, this.transloco,
                        this.transloco.translate('alert.createUpdate.created'));
                },
                error: (errorResponse) => {
                    utils.toastError(errorResponse, this.messageService, this.transloco)
                }
            })
    }

    private updateAlert(): void {
        const updatedAlertRawData = this.createUpdateAlertForm.getRawValue() as CreateUpdateAlertModel;
        const updateData = AlertUtils
            .GetUpdateAlertData(updatedAlertRawData, this.alertInitialValues as DetailedAlertView);
        this.alertService.updateAlert(updateData)
            .pipe(finalize(() => this.doingRequest = false))
            .subscribe({
                next: (_) => {
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
            { Display: this.transloco.translate("alert.frequency.twoHours"), Value: EAlertFrequency.TwoHours },
            { Display: this.transloco.translate("alert.frequency.fourHours"), Value: EAlertFrequency.FourHours },
            { Display: this.transloco.translate("alert.frequency.eightHours"), Value: EAlertFrequency.EightHours },
            { Display: this.transloco.translate("alert.frequency.twelveHours"), Value: EAlertFrequency.TwelveHours },
            {
                Display: this.transloco.translate("alert.frequency.twentyFourHours"),
                Value: EAlertFrequency.TwentyFourHours
            },
        ];

        this.ruleOptions = [
            { Display: this.transloco.translate("alert.rule.anyChanges"), Value: Rules.AnyChanges },
            { Display: this.transloco.translate("alert.rule.term"), Value: Rules.Term },
            { Display: this.transloco.translate("alert.rule.regex"), Value: Rules.Regex }
        ]

        this.notifyOnDisappearanceOptions = [
            { Display: this.transloco.translate("common.yes"), Value: true },
            { Display: this.transloco.translate("common.no"), Value: false }
        ]
    }

    private checkIfDataChanged(): void {
        this.dataChanged = this.alertInitialValues?.Name != this.inputFormName?.value?.trim() ||
            this.alertInitialValues?.Frequency != this.createUpdateAlertForm.get('frequency')?.value ||
            this.alertInitialValues?.Site.Name != this.inputFormSiteName?.value?.trim() ||
            this.alertInitialValues?.Site.Uri != this.inputFormSiteUri?.value?.trim() ||
            this.alertInitialValues?.Rule.Rule != this.createUpdateAlertForm.get('rule')?.value ||
            this.alertInitialValues?.Rule.Term != this.inputFormTermRule?.value?.trim() ||
            this.alertInitialValues?.Rule.RegexPattern != this.inputFormRegexRulePattern?.value?.trim() ||
            this.alertInitialValues?.Rule.NotifyOnDisappearance != this.inputFormRegexRuleNotifyOnDisappearance?.value
    }
}
