import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {AuthGuard} from "../core/guards/auth.guard";
import {SettingsPageComponent} from "./settings-page.component";
import {ProfilePageComponent} from "./profile-page/profile-page.component";
import {SecurityPageComponent} from "./security-page/security-page.component";

const settingsRoutes: Routes = [
  {
    path: '',
    component: SettingsPageComponent,
    canActivate: [AuthGuard],
    children:
      [
        {path: '', pathMatch: 'full', redirectTo: 'profile'},
        {path: 'profile', component: ProfilePageComponent},
        {path: 'security', component: SecurityPageComponent}
      ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(settingsRoutes)],
  exports: [RouterModule]
})
export class SettingsRoutingModule {
}
