import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {TopBarComponent} from './top-bar.component';
import {FormsModule} from "@angular/forms";
import {InputTextModule} from "primeng/inputtext";

@NgModule({
    declarations: [TopBarComponent],
    imports: [
        CommonModule,
        FormsModule,
        InputTextModule
    ],
    exports: [TopBarComponent]
})
export class TopBarModule {
}
