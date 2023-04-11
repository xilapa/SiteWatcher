import { HttpClient } from "@angular/common/http";
import { Component, HostListener, OnInit } from '@angular/core';
import { TranslocoService } from "@ngneat/transloco";
import { first } from "rxjs";
import { environment } from "../environments/environment";
import { LangUtils } from "./core/lang/lang.utils";
import { ThemeService } from "./core/theme/theme.service";
import { UserService } from "./core/user/user.service";

@Component({
    selector: 'sw-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit{
    title = 'SiteWatcher.UI';

    constructor(
        window: Window,
        translocoService: TranslocoService,
        readonly themeService: ThemeService,
        private readonly userService: UserService,
        private readonly httpClient: HttpClient
    ) {
        translocoService.setActiveLang(LangUtils.getCurrentBrowserLanguageFileName(window));
        themeService.loadUserTheme();
    }

    // Listen to storage events to update the user email confirmed status
    @HostListener('window:storage', ['$event'])
    onStorageChange(ev: StorageEvent) {
        this.userService.getUserInfoAndNotify().pipe(first()).subscribe();
    }

    ngOnInit(): void {
        // call the server on init to wake it up
        this.httpClient.get(environment.baseApiUrl).pipe(first()).subscribe();
    }

}
