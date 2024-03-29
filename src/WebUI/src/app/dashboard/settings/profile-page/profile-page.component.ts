import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from "@angular/forms";
import { NavigationStart, Router } from "@angular/router";
import { TranslocoService } from "@ngneat/transloco";
import { MessageService } from "primeng/api";
import { Observable, Subscription, finalize } from "rxjs";
import { invalidCharactersValidator } from "../../../common/validators/invalid-characters.validator";
import { DeviceService } from "../../../core/device/device.service";
import { UpdateUser, User } from "../../../core/interfaces";
import { LangUtils } from "../../../core/lang/lang.utils";
import { ELanguage } from "../../../core/lang/language";
import { ETheme } from "../../../core/theme/theme";
import { ThemeService } from "../../../core/theme/theme.service";
import { UserService } from "../../../core/user/user.service";
import { utils } from "../../../core/utils/utils";
import { LanguageOptions } from "../../../home/register/language-options";

@Component({
    selector: 'sw-profile-page',
    templateUrl: './profile-page.component.html',
    styleUrls: ['./profile-page.component.css']
})
export class ProfilePageComponent implements OnInit, OnDestroy {

    updateForm: FormGroup;
    user: User;
    initialUser: User;
    inputFormName: AbstractControl | null;
    inputFormEmail: AbstractControl | null;
    languageOptions = LanguageOptions;
    darkThemeEnabled: boolean;
    mobileScreen$: Observable<boolean>;
    dataChanged = false;
    darkThemeEnabledInitial: boolean;
    doingRequest: boolean;
    private userSub: Subscription | undefined;
    private langSub: Subscription | undefined;
    private themeSub: Subscription | undefined;
    private formSub: Subscription | undefined;
    private routerSub: Subscription | undefined;


    constructor(private readonly deviceService: DeviceService,
        private readonly formBuilder: FormBuilder,
        private readonly userService: UserService,
        private readonly translocoService: TranslocoService,
        private readonly themeService: ThemeService,
        private readonly messageService: MessageService,
        private readonly router: Router) {
        this.userSub = this.userService.getUser().subscribe(u => this.initialUser = this.user = u as User);
        this.mobileScreen$ = this.deviceService.isMobileScreen();
    }

    ngOnInit(): void {
        // if route changes, the theme and language will be reset
        this.routerSub = this.router.events.subscribe(
            event => {
                if (event instanceof NavigationStart) {
                    this.themeService.loadUserTheme();
                    this.translocoService.setActiveLang(
                        LangUtils.getLangFileName(this.userService.getCurrentUser()?.Language as ELanguage));
                }
            });

        this.darkThemeEnabledInitial = this.darkThemeEnabled = this.user.Theme != ETheme.light;
        this.updateForm = this.formBuilder.group(
            {
                name: [this.user.Name, [Validators.required, Validators.minLength(3), invalidCharactersValidator]],
                email: [this.user.Email, [Validators.required, Validators.email]],
                language: [this.user.Language, [Validators.required]],
                theme: [this.darkThemeEnabled]
            }
        )

        this.inputFormName = this.updateForm.get('name');
        this.inputFormEmail = this.updateForm.get('email');

        this.langSub = this.updateForm.get('language')?.valueChanges.subscribe(
            (lang: ELanguage) => this.translocoService.setActiveLang(LangUtils.getLangFileName(lang))
        );

        this.themeSub = this.updateForm.get('theme')?.valueChanges.subscribe(
            () => {
                this.themeService.toggleTheme()
                this.darkThemeEnabled = !this.darkThemeEnabled;
            }
        );

        this.formSub = this.updateForm.valueChanges.subscribe(() => this.checkIfDataChanged());
    }

    ngOnDestroy(): void {
        this.userSub?.unsubscribe();
        this.langSub?.unsubscribe();
        this.themeSub?.unsubscribe();
        this.formSub?.unsubscribe();
        this.routerSub?.unsubscribe();
    }

    checkIfDataChanged(): void {
        this.dataChanged = this.initialUser.Name != this.inputFormName?.value?.trim() ||
            this.initialUser.Email != this.inputFormEmail?.value?.trim() ||
            this.initialUser.Language != this.updateForm.get('language')?.value ||
            this.darkThemeEnabledInitial != this.darkThemeEnabled;
    }

    update(): void {
        this.doingRequest = true;
        const updateData = this.updateForm.getRawValue() as UpdateUser;
        updateData.name = updateData.name.trim();
        updateData.email = updateData.email.trim();
        updateData.theme = this.darkThemeEnabled ? ETheme.dark : ETheme.light;

        this.userService.update(updateData)
            .pipe(finalize(() => this.doingRequest = false))
            .subscribe({
                next: (u) => {                   
                    this.initialUser = this.userService.getCurrentUser() as User;
                    this.darkThemeEnabledInitial = this.darkThemeEnabled = this.user.Theme != ETheme.light;
                    this.updateForm.updateValueAndValidity();

                    const toastMessage = `${this.translocoService.translate('settings.security.successMessage')}
                    ${u.ConfirmationEmailSent ? this.translocoService.translate('settings.security.successMessageEmailSent') : ''}`;

                    utils.toastSuccess(this.messageService, this.translocoService, toastMessage);
                },
                error: (errorResponse) =>
                    utils.toastError(errorResponse, this.messageService,
                        this.translocoService)
            })
    }
}
