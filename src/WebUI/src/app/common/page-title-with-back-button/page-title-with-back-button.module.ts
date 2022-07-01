import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {PageTitleWithBackButtonComponent} from "./page-title-with-back-button.component";
import {RouterModule} from "@angular/router";
import {TranslocoModule} from "@ngneat/transloco";


@NgModule({
    declarations: [
        PageTitleWithBackButtonComponent
    ],
    exports: [
        PageTitleWithBackButtonComponent
    ],
    imports: [
        CommonModule,
        RouterModule,
        TranslocoModule
    ]
})
export class PageTitleWithBackButtonModule {
}
