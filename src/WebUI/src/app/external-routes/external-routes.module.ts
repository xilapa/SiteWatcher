import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ConfirmEmailComponent} from './confirm-email/confirm-email.component';
import {ExternalRoutesRoutingModule} from "./external-routes.routing.module";
import {LoadingPulseBubblesModule} from "../common/loading-pulse-bubbles/loading-pulse-bubbles.module";


@NgModule({
    declarations: [
        ConfirmEmailComponent
    ],
    imports: [
        CommonModule,
        ExternalRoutesRoutingModule,
        LoadingPulseBubblesModule
    ]
})
export class ExternalRoutesModule {
}
