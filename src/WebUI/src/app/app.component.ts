import {DOCUMENT} from '@angular/common';
import {Component, HostListener, Inject} from '@angular/core';
import {Router} from '@angular/router';
import {AuthUtils} from './core/auth/auth.utils';
import {ThemeService} from "./core/theme/theme.service";
import {LangUtils} from "./core/lang/lang.utils";
import {TranslocoService} from "@ngneat/transloco";
import {UserService} from "./core/user/user.service";

@Component({
    selector: 'sw-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    title = 'SiteWatcher.UI';

    constructor(
        @Inject(DOCUMENT) doc: Document,
        private window: Window,
        private translocoService: TranslocoService,
        router: Router,
        private readonly themeService: ThemeService,
        private readonly userService: UserService
    ) {
        translocoService.setActiveLang(LangUtils.getCurrentBrowserLanguageFileName(window));
        themeService.loadUserTheme();
        AuthUtils.checkAuthAndRedirect(doc.location.href, router);
    }

    // Listen to storage events to update the user email confirmed status
    @HostListener('window:storage', ['$event'])
    onStorageChange(ev: StorageEvent) {
        this.userService.decodeAndNotify();
    }
}
