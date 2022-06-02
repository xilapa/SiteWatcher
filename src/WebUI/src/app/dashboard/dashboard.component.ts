import {Component, OnDestroy, OnInit} from '@angular/core';
import {LangUtils} from "../core/lang/lang.utils";
import {UserService} from "../core/user/user.service";
import {TranslocoService} from "@ngneat/transloco";
import {ELanguage} from "../core/lang/language";
import {Subscription} from "rxjs";
import {ThemeService} from "../core/theme/theme.service";


@Component({
  selector: 'sw-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit, OnDestroy {

  private userSub: Subscription;

  constructor(private readonly userService: UserService,
              private readonly themeService: ThemeService,
              private readonly translocoService: TranslocoService) {
  }

  ngOnInit(): void {
    this.themeService.loadUserTheme();
    this.userSub = this.userService.getUser()
      .subscribe(user => {
        this.translocoService.setActiveLang(LangUtils.getLangFileName(user?.language as ELanguage));
      })
  }

  ngOnDestroy(): void {
    this.userSub?.unsubscribe();
  }


}
