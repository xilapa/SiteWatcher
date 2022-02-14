import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogInComponent } from './log-in/log-in.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';



@NgModule({
  declarations: [
    LogInComponent
  ],
  imports: [
    CommonModule,
    FontAwesomeModule
  ],
  exports: [LogInComponent]
})
export class HomeModule { }
