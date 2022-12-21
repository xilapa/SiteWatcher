import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { TranslocoModule } from "@ngneat/transloco";
import { ButtonModule } from "primeng/button";
import { BasePageLayoutModule } from '../common/base-page-layout/base-page-layout.module';
import { AboutComponent } from './about/about.component';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardComponent } from './dashboard.component';
import { TopBarModule } from './top-bar/top-bar.module';



@NgModule({
  declarations: [
    DashboardComponent,
    AboutComponent],
    imports: [
        CommonModule,
        TopBarModule,
        DashboardRoutingModule,
        TranslocoModule,
        ButtonModule,
        BasePageLayoutModule
    ]
})
export class DashboardModule { }
