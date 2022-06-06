import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {LoadingPulseBubblesComponent} from "./loading-pulse-bubbles.component";


@NgModule({
    declarations: [LoadingPulseBubblesComponent],
    imports: [
        CommonModule
    ],
    exports: [LoadingPulseBubblesComponent]
})
export class LoadingPulseBubblesModule {
}
