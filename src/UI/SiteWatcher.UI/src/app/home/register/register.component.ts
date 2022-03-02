import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { LanguageOptions } from 'src/app/core/dropdown-options/language-options';
import { UserRegister } from 'src/app/core/interfaces';
import { Data } from 'src/app/core/shared-data/shared-data';


@Component({
    selector: 'sw-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

    public registerForm: FormGroup;
    public languageOptions = LanguageOptions;
    public userRegister: UserRegister;


    constructor(private readonly formBuilder: FormBuilder,
        private readonly router: Router) {

        this.userRegister = Data.Get("register-data") as UserRegister;
        console.log(this.userRegister);

        //mock data
        this.userRegister = {
          email: "dirceu.sjr@gmail.com",
          googleId: "114086475315666159365",
          language: 1,
          name: "Dirceu Junior"
        }

        if (!this.userRegister) {
            this.router.navigateByUrl('/home');
            return;
        }
    }

    ngOnInit(): void {        
        this.registerForm = this.formBuilder.group(
            {
                name: [this.userRegister.name],
                email: [this.userRegister.email],
                language: [this.userRegister.language]
            }
        )
    }

    public register(): void {
        console.log("registrado")
    }



}
