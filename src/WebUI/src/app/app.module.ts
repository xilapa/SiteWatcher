import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {HttpClientModule, HTTP_INTERCEPTORS} from '@angular/common/http';
import {ToastModule} from 'primeng/toast';
import {MessageService} from 'primeng/api';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {CookieService} from 'ngx-cookie-service';

import {AppRoutingModule} from './app-routing.module';
import {AppComponent} from './app.component';
import {HomeModule} from './home/home.module';
import {AuthHeaderInterceptor} from './core/auth/auth-header.interceptor';
import {UnauthorizedInterceptor} from './core/auth/unauthorized.interceptor';
import {ContentTypeInterceptor} from './core/interceptors/content-type.interceptor';
import {TranslocoRootModule} from './transloco-root.module';


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
        TranslocoRootModule
    ],
    providers: [
        MessageService,
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
