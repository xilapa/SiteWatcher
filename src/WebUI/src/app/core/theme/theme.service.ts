import {Inject, Injectable} from '@angular/core';
import {LocalStorageService} from "../local-storage/local-storage.service";
import {DOCUMENT} from "@angular/common";
import {ETheme} from "./theme";
import {UserService} from '../user/user.service';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  private readonly docClassList: DOMTokenList;
  private readonly themeKey: string = 'theme';
  private readonly darkTheme: string = 'dark-theme';
  private readonly lightTheme: string = 'light-theme';
  private oldTheme: string | null;

  constructor(@Inject(DOCUMENT) private readonly doc: Document,
              private readonly localStorage: LocalStorageService,
              private readonly userService: UserService) {
    this.docClassList = doc.documentElement.classList;
  }

  public loadUserTheme(): void {
    this.userService.getUser()
      .subscribe(user => {
        let themeToLoad;
        if(!user)
          themeToLoad = this.localStorage.getItem(this.themeKey) as string;
        if(user && user.theme === ETheme.dark)
          themeToLoad = this.darkTheme;
        else if(user && user.theme === ETheme.light)
          themeToLoad = this.lightTheme;

        themeToLoad && this.docClassList.add(themeToLoad);
      });
  }

  public toggleTheme(): void {
    let newTheme: string;

    if (this.docClassList.contains(this.lightTheme)) {
      this.oldTheme = this.lightTheme;
      newTheme = this.darkTheme;
    } else if (this.docClassList.contains(this.darkTheme)) {
      this.oldTheme = this.darkTheme;
      newTheme = this.lightTheme;
    } else {
      if (this.doc.defaultView?.matchMedia('(prefers-color-scheme: dark)').matches) {
        newTheme = this.lightTheme;
      } else {
        newTheme = this.darkTheme;
      }
    }

        this.docClassList.add(newTheme);
        this.oldTheme && this.docClassList.remove(this.oldTheme);

        this.localStorage.setItem(this.themeKey, newTheme);
    }

  public getCurrentTheme(): ETheme {
    let theme = ETheme.light;

    if (this.docClassList.contains(this.darkTheme) ||
      this.doc.defaultView?.matchMedia('(prefers-color-scheme: dark)').matches) {
      theme = ETheme.dark;
    }
    console.log(theme);
    return theme;
  }
}
