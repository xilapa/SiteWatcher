import {Component, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, Validators} from "@angular/forms";
import {invalidCharactersValidator} from "../../../common/validators/invalid-characters.validator";
import {Location} from "@angular/common";
import {DeviceService} from "../../../core/device/device.service";
import {UserService} from "../../../core/user/user.service";
import {User, UpdateUser} from "../../../core/interfaces";
import {LanguageOptions} from "../../../home/register/language-options";
import {LangUtils} from "../../../core/lang/lang.utils";
import {ELanguage} from "../../../core/lang/language";
import {TranslocoService} from "@ngneat/transloco";
import {finalize, Observable, Subscription} from "rxjs";
import {ThemeService} from "../../../core/theme/theme.service";
import {ETheme} from "../../../core/theme/theme";
import {MessageService} from "primeng/api";
import {NavigationStart, Router} from "@angular/router";
import {utils} from "../../../core/utils/utils";

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


    constructor(private readonly location: Location,
                private readonly deviceService: DeviceService,
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
                        LangUtils.getLangFileName(this.userService.getCurrentUser()?.language as ELanguage));
                }
            });

        this.darkThemeEnabledInitial = this.darkThemeEnabled = this.user.theme != ETheme.light;
        this.updateForm = this.formBuilder.group(
            {
                name: [this.user.name, [Validators.required, Validators.minLength(3), invalidCharactersValidator]],
                email: [this.user.email, [Validators.required, Validators.email]],
                language: [this.user.language, [Validators.required]],
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
        this.dataChanged = this.initialUser.name != this.inputFormName?.value?.trim() ||
            this.initialUser.email != this.inputFormEmail?.value?.trim() ||
            this.initialUser.language != this.updateForm.get('language')?.value ||
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
                next: (resp) => {
                    this.userService.setToken(resp.Token);

                    this.initialUser = this.userService.getCurrentUser() as User;
                    this.darkThemeEnabledInitial = this.darkThemeEnabled = this.user.theme != ETheme.light;
                    this.updateForm.updateValueAndValidity();

                    const toastMessage = `${this.translocoService.translate('settings.security.successMessage')}
                    ${resp.ConfirmationEmailSend ?
                                this.translocoService.translate('settings.security.successMessageEmailSent') : ''}`;

                    utils.toastSuccess(this.messageService, this.translocoService, toastMessage);
                },
                error: (errorResponse) =>
                    utils.toastError(errorResponse, this.messageService,
                        this.translocoService)
            })
    }
}
