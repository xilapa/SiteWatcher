import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LogInComponent } from './log-in/log-in.component';
import { HomeRoutingModule } from './home-routing.module';
import { AuthComponent } from './auth/auth.component';
import { HomeComponent } from './home.component';
import { ButtonModule } from 'primeng/button';
import { RegisterComponent } from './register/register.component';
import { ReactiveFormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';

@NgModule({
  declarations: [
    HomeComponent,
    LogInComponent,
    AuthComponent,
    RegisterComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    HomeRoutingModule
  ],
  exports: [LogInComponent]
})
export class HomeModule { }
