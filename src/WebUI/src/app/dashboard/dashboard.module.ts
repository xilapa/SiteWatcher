import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard.component';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { TopBarModule } from './top-bar/top-bar.module';
import {TranslocoModule} from "@ngneat/transloco";
import {ButtonModule} from "primeng/button";



@NgModule({
  declarations: [DashboardComponent],
    imports: [
        CommonModule,
        TopBarModule,
        DashboardRoutingModule,
        TranslocoModule,
        ButtonModule
    ]
})
export class DashboardModule { }
