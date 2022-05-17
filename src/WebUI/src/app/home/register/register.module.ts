import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RegisterComponent} from './register.component';
import {RegisterRoutingModule} from './register-routing.module';
import {InputTextModule} from 'primeng/inputtext';
import {DropdownModule} from 'primeng/dropdown';
import {ReactiveFormsModule} from '@angular/forms';
import {InlineInputErrorMsgModule} from 'src/app/common/inline-input-error-msg/inline-input-error-msg.module';
import {ButtonModule} from 'primeng/button';
import {TranslocoModule} from "@ngneat/transloco";


@NgModule({
    declarations: [RegisterComponent],
    imports: [
        CommonModule,
        ReactiveFormsModule,
        ButtonModule,
        DropdownModule,
        InputTextModule,
        InlineInputErrorMsgModule,
        RegisterRoutingModule,
        TranslocoModule
    ]
})
export class RegisterModule {
}
