import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AlertItemComponent} from './alert-item/alert-item.component';
import {AlertListComponent} from "./alert-list.component";
import {AlertListRoutingModule} from "./alert-list.routing.module";
import {TranslocoModule} from "@ngneat/transloco";
import {InfiniteScrollModule} from "ngx-infinite-scroll";
import {DynamicDialogModule} from "primeng/dynamicdialog";
import {AlertDetailsModule} from "../alert-details/alert-details.module";
import {AlertDetailsComponent} from "../alert-details/alert-details.component";

@NgModule({
    declarations: [
        AlertItemComponent,
        AlertListComponent
    ],
    imports: [
        CommonModule,
        AlertListRoutingModule,
        TranslocoModule,
        InfiniteScrollModule,
        DynamicDialogModule,
        AlertDetailsModule
    ],
    entryComponents:[
        AlertDetailsComponent
    ]
})
export class AlertListModule {
}
