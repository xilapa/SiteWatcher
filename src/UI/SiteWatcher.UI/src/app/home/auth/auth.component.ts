import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/core/auth/auth.service';
import { EAuthTask } from 'src/app/core/enums';
import { ApiResponse } from 'src/app/core/interfaces';
import { Data } from 'src/app/core/shared-data/shared-data';
import { MessageService } from 'primeng/api';
import { UserService } from 'src/app/core/user/user.service';

@Component({
    selector: 'sw-auth',
    templateUrl: './auth.component.html',
    styleUrls: ['./auth.component.css']
})
export class AuthComponent implements OnInit {

    constructor(private readonly authService: AuthService,
        private readonly router: Router,
        private readonly messageService: MessageService,
        private readonly userService: UserService) { }

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

                    if(response.Result.Task == EAuthTask.Register){
                        this.userService.setUserRegisterData(response.Result.Token);
                        this.router.navigateByUrl('/home/register');
                    }

                    if(response.Result.Task == EAuthTask.Login){
                        this.userService.setToken(response.Result.Token);
                        this.authService.redirecLoggedUser();
                    }

                },
                error: (errorResponse) => {
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
