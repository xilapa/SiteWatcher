import {DOCUMENT} from '@angular/common';
import {Component, Inject} from '@angular/core';
import {Router} from '@angular/router';
import {AuthUtils} from './core/auth/auth.utils';
import {ThemeService} from "./core/theme/theme.service";
import {LangUtils} from "./core/lang/lang.utils";
import {TranslocoService} from "@ngneat/transloco";

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
        private readonly themeService: ThemeService
    ) {
        translocoService.setActiveLang(LangUtils.getCurrentBrowserLanguageFileName(window));
        themeService.loadSavedTheme();
        AuthUtils.checkAuthAndRedirect(doc.location.href, router);
    }

    //TODO: remover daqui, está apenas para teste
    public toogleTheme() {
        this.themeService.toggleTheme();
    }
}
