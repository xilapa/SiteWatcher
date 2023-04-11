import { Component, OnDestroy, OnInit } from '@angular/core';
import { NavigationEnd, Router } from "@angular/router";
import { TranslocoService } from "@ngneat/transloco";
import { Subscription } from "rxjs";
import { AlertService } from "../alerts/service/alert.service";
import { LangUtils } from "../core/lang/lang.utils";
import { ELanguage } from "../core/lang/language";
import { ThemeService } from "../core/theme/theme.service";
import { UserService } from "../core/user/user.service";

@Component({
    selector: 'sw-dashboard',
    templateUrl: './dashboard.component.html',
    styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {

    private userSub: Subscription;
    private routeEventSub: Subscription;
    private searchResultsSub: Subscription;
    showCreateAlertButton = true;
    searching = false;
    isSearchResult = false;

    constructor(private readonly userService: UserService,
                private readonly themeService: ThemeService,
                private readonly translocoService: TranslocoService,
                private readonly router: Router,
                private readonly alertService: AlertService) {
    }

    ngOnInit(): void {
        this.searchResultsSub = this.alertService
            .searchResults()
            .subscribe(alerts => this.isSearchResult = !!alerts);

        this.routeEventSub = this.router.events.subscribe(event => {
            if(event instanceof NavigationEnd){
                const slashIndex = event?.urlAfterRedirects.lastIndexOf('/');
                const activePage = this.router.url.substring(slashIndex + 1);
                this.showCreateAlertButton = activePage == 'dash';
                if(activePage != 'dash'){
                    this.clearSearchResults();
                }
            }
        })

        this.themeService.loadUserTheme();
        this.userSub = this.userService.getUser()
            .subscribe(user => {
                if(!user) return;
                this.translocoService
                    .setActiveLang(LangUtils.getLangFileName(user?.Language as ELanguage));
            })
    }

    ngOnDestroy(): void {
        this.userSub?.unsubscribe();
        this.routeEventSub?.unsubscribe();
        this.searchResultsSub?.unsubscribe();
    }

    onSearch(isSearching : boolean){
        this.searching = isSearching;
    }

    clearSearchResults(){
        this.alertService
            .clearSearchResults();
    }
}
