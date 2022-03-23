import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RegisterPageGuard } from 'src/app/core/guards/register-page.guard';
import { RegisterComponent } from './register.component';


const registerRoutes: Routes = [ { path: '', component: RegisterComponent, canActivate: [RegisterPageGuard]} ];

@NgModule({
  imports: [RouterModule.forChild(registerRoutes)],
  exports: [RouterModule]
})
export class RegisterRoutingModule { }