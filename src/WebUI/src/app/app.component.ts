import {DOCUMENT} from '@angular/common';
import {Component, Inject} from '@angular/core';
import {Router} from '@angular/router';
import {checkAuthRedirect} from './core/auth/auth.utils';
import {ThemeService} from "./core/theme/theme.service";

@Component({
    selector: 'sw-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    title = 'SiteWatcher.UI';

    constructor(@Inject(DOCUMENT) doc: Document, router: Router, private readonly themeService: ThemeService) {
        themeService.loadSavedTheme();
        checkAuthRedirect(doc.location.href, router);
    }

    //TODO: remover daqui, está apenas para teste
    public toogleTheme() {
        this.themeService.toggleTheme();
    }
}
