import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ConfirmEmailComponent} from './confirm-email/confirm-email.component';
import {ExternalRoutesRoutingModule} from "./external-routes.routing.module";
import {LoadingPulseBubblesModule} from "../common/loading-pulse-bubbles/loading-pulse-bubbles.module";
import {ReactivateAccountComponent} from "./reactivate-account/reactivate-account.component";
import {TranslocoModule} from "@ngneat/transloco";


@NgModule({
    declarations: [
        ConfirmEmailComponent,
        ReactivateAccountComponent
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
