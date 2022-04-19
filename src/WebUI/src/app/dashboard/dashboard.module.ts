import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardComponent } from './dashboard.component';
import { DashboardRoutingModule } from './dashboard-routing.module';
import { TopBarModule } from './top-bar/top-bar.module';



@NgModule({
  declarations: [DashboardComponent],
  imports: [
    CommonModule,
    TopBarModule,
    DashboardRoutingModule
  ]
})
export class DashboardModule { }
