import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import {ToastModule} from 'primeng/toast';
import {ConfirmationService, MessageService} from 'primeng/api';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {CookieService} from 'ngx-cookie-service';

import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {HomeModule} from './home/home.module';
import {ContentTypeInterceptor} from './core/interceptors/content-type.interceptor';
import {TranslocoRootModule} from './transloco-root.module';
import {AuthHeaderInterceptor} from "./core/auth/interceptors/auth-header.interceptor";
import {UnauthorizedInterceptor} from "./core/auth/interceptors/unauthorized.interceptor";
import {ConfirmDialogModule} from "primeng/confirmdialog";


@NgModule({
    declarations: [
        AppComponent
    ],
    imports: [
        BrowserModule,
        HttpClientModule,
        HomeModule,
        AppRoutingModule,
        ToastModule,
        BrowserAnimationsModule,
        TranslocoRootModule,
        ConfirmDialogModule
    ],
    providers: [
        MessageService,
        ConfirmationService,
        CookieService,
        {
            provide: HTTP_INTERCEPTORS,
            useClass: ContentTypeInterceptor,
            multi: true
        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthHeaderInterceptor,
            multi: true

        },
        {
            provide: HTTP_INTERCEPTORS,
            useClass: UnauthorizedInterceptor,
            multi: true

        },
        {
            provide: Window,
            useValue: window
        }
    ],
    bootstrap: [AppComponent]
})
export class AppModule {
}
