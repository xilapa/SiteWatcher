import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginPageGuard } from '../core/guards/login-page.guard';
import { AuthComponent } from './auth/auth.component';
import { HomeComponent } from './home.component';
import { LogInComponent } from './log-in/log-in.component';

const homeRoutes: Routes = [
  {
    path: 'home',
    component: HomeComponent,
    children:
      [ 
        { path: '', pathMatch: 'full', redirectTo: 'login'},
        { path: 'login', component: LogInComponent, canActivate: [LoginPageGuard] },
        { path: 'auth', component: AuthComponent }
      ]
  },
  { path: 'register', loadChildren: () => import('./register/register.module').then(m => m.RegisterModule)}
];

@NgModule({
  imports: [RouterModule.forChild(homeRoutes)],
  exports: [RouterModule]
})
export class HomeRoutingModule { }