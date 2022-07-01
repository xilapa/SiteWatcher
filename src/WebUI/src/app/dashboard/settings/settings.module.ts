import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ProfilePageComponent} from './profile-page/profile-page.component';
import {SecurityPageComponent} from './security-page/security-page.component';
import {SettingsPageComponent} from "./settings-page.component";
import {SettingsRoutingModule} from "./settings-routing.module";
import {InputTextModule} from "primeng/inputtext";
import {ReactiveFormsModule} from "@angular/forms";
import {InlineInputErrorMsgModule} from "../../common/inline-input-error-msg/inline-input-error-msg.module";
import {TranslocoModule} from "@ngneat/transloco";
import {DropdownModule} from "primeng/dropdown";
import {ButtonModule} from "primeng/button";
import {InputSwitchModule} from "primeng/inputswitch";
import {
    PageTitleWithBackButtonModule
} from "../../common/page-title-with-back-button/page-title-with-back-button.module";
import {BasePageLayoutModule} from "../../common/base-page-layout/base-page-layout.module";


@NgModule({
    declarations: [
        SettingsPageComponent,
        ProfilePageComponent,
        SecurityPageComponent
    ],
    imports: [
        CommonModule,
        SettingsRoutingModule,
        InputTextModule,
        ReactiveFormsModule,
        InlineInputErrorMsgModule,
        TranslocoModule,
        DropdownModule,
        ButtonModule,
        InputSwitchModule,
        PageTitleWithBackButtonModule,
        BasePageLayoutModule
    ]
})
export class SettingsModule {
}
