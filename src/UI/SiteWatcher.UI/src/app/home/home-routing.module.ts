import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthComponent } from './auth/auth.component';
import { HomeComponent } from './home.component';
import { LogInComponent } from './log-in/log-in.component';
import { RegisterComponent } from './register/register.component';


const homeRoutes: Routes = [
  {
    path: 'home',
    component: HomeComponent,
    children:
      [ 
        { path: '', pathMatch: 'full', redirectTo: 'login'},
        { path: 'login', component: LogInComponent },
        { path: 'auth', component: AuthComponent }
      ]
  },
  { path: 'register', component: RegisterComponent }

];

@NgModule({
  imports: [RouterModule.forChild(homeRoutes)],
  exports: [RouterModule]
})
export class HomeRoutingModule { }