import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { TranslocoModule } from "@ngneat/transloco";
import { LoadingPulseBubblesModule } from "../common/loading-pulse-bubbles/loading-pulse-bubbles.module";
import { ConfirmEmailComponent } from './confirm-email/confirm-email.component';
import { ExternalRoutesRoutingModule } from "./external-routes.routing.module";
import { PrivacyPolicyComponent } from './privacy-policy/privacy-policy.component';
import { ReactivateAccountComponent } from "./reactivate-account/reactivate-account.component";


@NgModule({
    declarations: [
        ConfirmEmailComponent,
        ReactivateAccountComponent,
        PrivacyPolicyComponent
    ],
    imports: [
        CommonModule,
        ExternalRoutesRoutingModule,
        LoadingPulseBubblesModule,
        TranslocoModule
    ]
})
export class ExternalRoutesModule {
}
