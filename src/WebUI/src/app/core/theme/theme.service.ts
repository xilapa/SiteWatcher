import { DOCUMENT } from "@angular/common";
import { Inject, Injectable } from '@angular/core';
import { LocalStorageService } from "../local-storage/local-storage.service";
import { UserService } from '../user/user.service';
import { ETheme } from "./theme";

@Injectable({
  providedIn: 'root'
})
export class ThemeService {

  private readonly docClassList: DOMTokenList;
  static readonly themeKey: string = 'theme';
  private readonly darkTheme: string = 'dark-theme';
  private readonly lightTheme: string = 'light-theme';
  private oldTheme: string | null;

  constructor(@Inject(DOCUMENT) private readonly doc: Document,
              private readonly localStorage: LocalStorageService,
              private readonly userService: UserService) {
    this.docClassList = doc.documentElement.classList;
  }

  public loadUserTheme(): void {
    let user = this.userService.getCurrentUser();
    let themeToLoad;
    let themeToRemove;
    if (!user)
      themeToLoad = this.localStorage.getItem(ThemeService.themeKey) as string;
    if (user && user.Theme === ETheme.dark) {
      themeToLoad = this.darkTheme;
      themeToRemove = this.lightTheme;
    } else if (user && user.Theme === ETheme.light) {
      themeToLoad = this.lightTheme;
      themeToRemove = this.darkTheme;
    }

    themeToLoad && this.docClassList.add(themeToLoad);
    themeToLoad && this.localStorage.setItem(ThemeService.themeKey, themeToLoad);
    themeToRemove && this.docClassList.remove(themeToRemove);
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

    this.localStorage.setItem(ThemeService.themeKey, newTheme);
  }

  public getCurrentTheme(): ETheme {
    let theme = ETheme.light;

    if (this.docClassList.contains(this.darkTheme) ||
      this.doc.defaultView?.matchMedia('(prefers-color-scheme: dark)').matches) {
      theme = ETheme.dark;
    }
    return theme;
  }
}
