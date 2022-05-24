import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {TopBarComponent} from './top-bar.component';
import {FormsModule} from "@angular/forms";
import {InputTextModule} from "primeng/inputtext";
import {SideBarModule} from "../../common/side-bar/side-bar.module";
import {TranslocoModule} from "@ngneat/transloco";

@NgModule({
    declarations: [TopBarComponent],
    imports: [
        CommonModule,
        FormsModule,
        InputTextModule,
        SideBarModule,
        TranslocoModule
    ],
    exports: [TopBarComponent]
})
export class TopBarModule {
}
