import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AlertDetailsComponent} from "./alert-details.component";
import {TranslocoModule} from "@ngneat/transloco";

@NgModule({
    declarations: [
        AlertDetailsComponent
    ],
    imports: [
        CommonModule,
        TranslocoModule
    ],
    exports: [
        AlertDetailsComponent
    ]
})
export class AlertDetailsModule {
}
