import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {ProfilePageComponent} from './profile-page/profile-page.component';
import {SecurityPageComponent} from './security-page/security-page.component';
import {SettingsPageComponent} from "./settings-page.component";
import {SettingsRoutingModule} from "./settings-routing.module";


@NgModule({
    declarations: [
        SettingsPageComponent,
        ProfilePageComponent,
        SecurityPageComponent
    ],
  imports: [
    CommonModule,
    SettingsRoutingModule
  ]
})
export class SettingsModule {
}
