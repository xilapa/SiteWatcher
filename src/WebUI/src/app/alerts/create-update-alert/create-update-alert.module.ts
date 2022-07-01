import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {CreateUpdateAlertComponent} from "./create-update-alert.component";
import {CreateUpdateAlertRoutingModule} from "./create-update-alert.routing.module";
import {InlineInputErrorMsgModule} from "../../common/inline-input-error-msg/inline-input-error-msg.module";
import {ReactiveFormsModule} from "@angular/forms";
import {TranslocoModule} from "@ngneat/transloco";
import {DropdownModule} from "primeng/dropdown";
import {InputTextModule} from "primeng/inputtext";
import {
    PageTitleWithBackButtonModule
} from "../../common/page-title-with-back-button/page-title-with-back-button.module";
import {ButtonModule} from "primeng/button";
import {BasePageLayoutModule} from "../../common/base-page-layout/base-page-layout.module";

@NgModule({
    declarations: [
        CreateUpdateAlertComponent
    ],
    imports: [
        CommonModule,
        CreateUpdateAlertRoutingModule,
        InlineInputErrorMsgModule,
        ReactiveFormsModule,
        TranslocoModule,
        DropdownModule,
        InputTextModule,
        PageTitleWithBackButtonModule,
        ButtonModule,
        BasePageLayoutModule
    ]
})
export class CreateUpdateAlertModule {
}
