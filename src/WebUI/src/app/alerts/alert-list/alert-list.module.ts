import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AlertItemComponent} from './alert-item/alert-item.component';
import {AlertListComponent} from "./alert-list.component";
import {AlertListRoutingModule} from "./alert-list.routing.module";
import {TranslocoModule} from "@ngneat/transloco";
import {InfiniteScrollModule} from "ngx-infinite-scroll";

@NgModule({
    declarations: [
        AlertItemComponent,
        AlertListComponent
    ],
    imports: [
        CommonModule,
        AlertListRoutingModule,
        TranslocoModule,
        InfiniteScrollModule
    ]
})
export class AlertListModule {
}
