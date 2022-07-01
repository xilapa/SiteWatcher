import {Component, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, Validators} from '@angular/forms';
import {Router} from "@angular/router";
import {DropdownOption} from "../../core/interfaces/dropdown-option";
import {AlertFrequency} from "../common/alert-frequency";
import {TranslocoService} from "@ngneat/transloco";
import { WatchMode } from '../common/watch-mode';
import {Subscription} from "rxjs";
import {uriValidator} from "../../common/validators/uri.validator";
import {watchModeTermValidator} from "../../common/validators/watch-mode-term.validator";

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
    private activePage: string;
    private watchModeSub : Subscription | undefined;

    constructor(private readonly router: Router,
                private readonly formBuilder: FormBuilder,
                private readonly transloco: TranslocoService) {
        this.frequencyOptions = [
            {Display: transloco.translate("alert.frequency.twoHours"), Value: AlertFrequency.TwoHours},
            {Display: transloco.translate("alert.frequency.fourHours"), Value: AlertFrequency.FourHours},
            {Display: transloco.translate("alert.frequency.eightHours"), Value: AlertFrequency.EightHours},
            {Display: transloco.translate("alert.frequency.twelveHours"), Value: AlertFrequency.TwelveHours},
            {Display: transloco.translate("alert.frequency.twentyFourHours"), Value: AlertFrequency.TwentyFourHours},
        ];

        this.watchModeOptions = [
            {Display: transloco.translate("alert.watchMode.anyChanges"), Value: WatchMode.AnyChanges},
            {Display: transloco.translate("alert.watchMode.term"), Value: WatchMode.Term}
        ]
    }

    ngOnDestroy(): void {
        this.watchModeSub?.unsubscribe();
    }

    ngOnInit(): void {
        const slashIndex = this.router.url.lastIndexOf('/');
        this.activePage = this.router.url.substring(slashIndex + 1);
        this.pageTitleTranslationKey = CreateUpdateAlertComponent.getTranslationKey(this.activePage);

        this.createUpdateAlertForm = this.formBuilder.group(
            {
                name: [, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                frequency: [AlertFrequency.TwoHours, [Validators.required]],
                'site-name': [, [Validators.required, Validators.minLength(3), Validators.maxLength(64)]],
                'site-uri': [, [Validators.required, uriValidator]],
                'watch-mode': [WatchMode.AnyChanges, [Validators.required]],
                term: [, watchModeTermValidator]
            }
        );

        this.inputFormName = this.createUpdateAlertForm.get('name');
        this.inputFormSiteName = this.createUpdateAlertForm.get('site-name');
        this.inputFormSiteUri = this.createUpdateAlertForm.get('site-uri');
        this.inputFormWatchModeTerm = this.createUpdateAlertForm.get('term');
        this.watchModeSub = this.createUpdateAlertForm.get('watch-mode')?.valueChanges
            .subscribe((watchMode: WatchMode) => {
                    this.termWatchModeSelected = WatchMode.Term == watchMode;
                    if (!this.termWatchModeSelected)
                        this.inputFormWatchModeTerm?.reset();
            });

    }

    private static getTranslationKey(pageName: string): string {
        return `alert.createUpdate.${pageName}Title`
    }

    public send(): void {
        console.log("enviado")
    }
}
