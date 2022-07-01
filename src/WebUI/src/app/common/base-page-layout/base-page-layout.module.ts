import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BasePageLayoutComponent } from './base-page-layout.component';
import {PageTitleWithBackButtonModule} from "../page-title-with-back-button/page-title-with-back-button.module";
import {TranslocoModule} from "@ngneat/transloco";



@NgModule({
    declarations: [
        BasePageLayoutComponent
    ],
    exports: [
        BasePageLayoutComponent
    ],
    imports: [
        CommonModule,
        PageTitleWithBackButtonModule,
        TranslocoModule
    ]
})
export class BasePageLayoutModule { }
