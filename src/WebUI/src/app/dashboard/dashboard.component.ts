import {Component, OnDestroy, OnInit} from '@angular/core';
import {LangUtils} from "../core/lang/lang.utils";
import {UserService} from "../core/user/user.service";
import {TranslocoService} from "@ngneat/transloco";
import {ELanguage} from "../core/lang/language";
import {Subscription} from "rxjs";
import {ThemeService} from "../core/theme/theme.service";
import {NavigationEnd, Router} from "@angular/router";

@Component({
    selector: 'sw-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {

    private userSub: Subscription;
    private routeEventSub: Subscription;
    showCreateAlertButton = true;

    constructor(private readonly userService: UserService,
                private readonly themeService: ThemeService,
                private readonly translocoService: TranslocoService,
                private readonly router: Router) {
    }

    ngOnInit(): void {

        this.routeEventSub = this.router.events.subscribe(event => {
            if(event instanceof NavigationEnd){
                const slashIndex = event?.urlAfterRedirects.lastIndexOf('/');
                const activePage = this.router.url.substring(slashIndex + 1);
                this.showCreateAlertButton = activePage == 'dash';
            }
        })

        this.themeService.loadUserTheme();
        this.userSub = this.userService.getUser()
            .subscribe(user => {
                if(!user) return;
                this.translocoService
                    .setActiveLang(LangUtils.getLangFileName(user?.language as ELanguage));
            })
    }

    ngOnDestroy(): void {
        this.userSub?.unsubscribe();
        this.routeEventSub?.unsubscribe();
    }
}
