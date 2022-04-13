/* eslint-disable no-unused-vars */
/* eslint-disable @typescript-eslint/unbound-method */
import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { invalidCharactersValidator } from 'src/app/common/validators/invalid-characters.validator';
import { AuthService } from 'src/app/core/auth/auth.service';
import { LanguageOptions } from 'src/app/core/dropdown-options/language-options';
import { ApiResponse, UserRegister } from 'src/app/core/interfaces';
import { UserService } from 'src/app/core/user/user.service';


@Component({
    selector: 'sw-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

    public registerForm: FormGroup;
    public languageOptions = LanguageOptions;
    private userRegisterInitialValues: UserRegister;

    public inputFormName: AbstractControl | null;
    public nameMinLenghtErrorMsg = "Username length should be at least 3.";
    public nameinvalidCharactersErrorMsg = "Username must only have letters.";

    public inputFormEmail: AbstractControl | null;
    public emailValidationErrorMsg = "Email is not valid.";


    constructor(
        private readonly formBuilder: FormBuilder,
        private readonly authService: AuthService,
        private readonly userService: UserService,
        private readonly messageService: MessageService) {        
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
    }

    public register(): void {
        const registerData = this.registerForm.getRawValue() as UserRegister;
        this.authService
                .register(registerData)
                .subscribe({
                    next: (resp) =>{
                        this.userService.setToken(resp.Result);
                        this.userService.removeUserRegisterData();
                        this.authService.redirecLoggedUser();
                    },
                    error: (errorResponse) => {
                        this.messageService.add(
                            {
                                severity: 'error',
                                summary: 'Error',
                                // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
                                detail: (errorResponse.error as ApiResponse<null>).Messages.join("; "),
                                sticky: true,
                                closable: true
                            }
                        )
                    }
                });
    }
}
