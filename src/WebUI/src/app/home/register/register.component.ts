import {Component, OnDestroy, OnInit} from '@angular/core';
import {AbstractControl, FormBuilder, FormGroup, Validators} from '@angular/forms';
import {MessageService} from 'primeng/api';
import {invalidCharactersValidator} from 'src/app/common/validators/invalid-characters.validator';
import {UserService} from 'src/app/core/user/user.service';
import {TranslocoService} from "@ngneat/transloco";
import {LangUtils} from "../../core/lang/lang.utils";
import {Subscription} from "rxjs";
import {LanguageOptions} from "./language-options";
import {ELanguage} from "../../core/lang/language";
import {UserRegister} from "../../core/auth/user-register";
import {AuthService} from "../../core/auth/service/auth.service";
import {ThemeService} from "../../core/theme/theme.service";
import {utils} from "../../core/utils/utils";

@Component({
    selector: 'sw-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit, OnDestroy {

    public registerForm: FormGroup;
    public languageOptions = LanguageOptions;
    private userRegisterInitialValues: UserRegister;
    public inputFormName: AbstractControl | null;
    public inputFormEmail: AbstractControl | null;
    private langSub: Subscription | undefined;

    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly authService: AuthService,
        private readonly userService: UserService,
        private readonly messageService: MessageService,
        private readonly translocoService: TranslocoService,
        private readonly themeService: ThemeService
    ) {
        this.userRegisterInitialValues = this.userService.getUserRegisterData();
    }

    ngOnInit(): void {
        this.registerForm = this.formBuilder.group(
            {
                name: [this.userRegisterInitialValues.name, [Validators.required, Validators.minLength(3), invalidCharactersValidator]],
                email: [this.userRegisterInitialValues.email, [Validators.required, Validators.email]],
                language: [this.userRegisterInitialValues.language, [Validators.required]]
            }
        );

        this.inputFormName = this.registerForm.get('name');
        this.inputFormEmail = this.registerForm.get('email');

        this.translocoService.setActiveLang(LangUtils.getLangFileName(this.userRegisterInitialValues.language));
        this.langSub = this.registerForm.get('language')?.valueChanges.subscribe(
            (lang: ELanguage) => this.translocoService.setActiveLang(LangUtils.getLangFileName(lang))
        );
    }

    ngOnDestroy(): void {
        this.langSub?.unsubscribe();
    }

    public register(): void {
        const registerData = this.registerForm.getRawValue() as UserRegister;
        registerData.theme = this.themeService.getCurrentTheme();
        this.userService
            .register(registerData)
            .subscribe({
                next: (resp) => {
                    this.userService.setToken(resp.Result);
                    this.userService.removeUserRegisterData();
                    this.userService.redirecLoggedUser();
                },
                error: (errorResponse) => {
                    utils.errorToast(errorResponse, this.messageService,
                        this.translocoService)
                }
            });
    }
}
