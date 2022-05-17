import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {LogInComponent} from './log-in/log-in.component';
import {HomeRoutingModule} from './home-routing.module';
import {AuthComponent} from './auth/auth.component';
import {HomeComponent} from './home.component';
import {ButtonModule} from 'primeng/button';
import {TranslocoModule} from "@ngneat/transloco";

@NgModule({
    declarations: [
        HomeComponent,
        LogInComponent,
        AuthComponent
    ],
    imports: [
        CommonModule,
        ButtonModule,
        HomeRoutingModule,
        TranslocoModule
    ],
    exports: [LogInComponent]
})
export class HomeModule {
}
