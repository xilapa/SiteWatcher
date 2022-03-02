import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/auth/auth.service';
import { EAuthTask, ELanguage } from 'src/app/core/enums';
import { ApiResponse, UserRegister } from 'src/app/core/interfaces';
import { Data } from 'src/app/core/shared-data/shared-data';
import { MessageService } from 'primeng/api';
import jwt_decode from "jwt-decode";

@Component({
    selector: 'sw-auth',
    templateUrl: './auth.component.html',
    styleUrls: ['./auth.component.css']
})
export class AuthComponent implements OnInit {

    constructor(private readonly authService: AuthService,
        private readonly router: Router,
        private readonly messageService: MessageService) { }

    ngOnInit(): void {
        const url = Data.GetAndRemove('authURL') as URL;
        if (url == null) {
            this.router.navigateByUrl('/home');
            return;
        }

        const state = url.searchParams.get('state') as string;
        const code = url.searchParams.get('code') as string;
        const scope = url.searchParams.get('scope') as string;

        this.authService.authenticate(state, code, scope)
            .subscribe({
                next: (response) => {
                    if(response.Result.Task == EAuthTask.Login)
                        console.log("redirect para dashboard")
                    
                    if(response.Result.Task == EAuthTask.Register) {
                        const userRegister = jwt_decode(response.Result.Token) as UserRegister;
                        userRegister.language = parseInt(userRegister.language as any) as ELanguage;
                        Data.Share("register-data", userRegister);
                        this.router.navigateByUrl('/home/register');
                    }

                },
                error: (errorResponse) => {
                    console.log((errorResponse.error as ApiResponse<null>).Messages)
                    this.messageService.add(
                        {
                            severity: 'error',
                            summary: 'Error',
                            detail: (errorResponse.error as ApiResponse<null>).Messages.join("; "),
                            sticky: true,
                            closable: true
                        }
                    )
                    this.router.navigateByUrl('/home');
                }
            });
    }

}
