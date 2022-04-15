import {Inject, Injectable} from '@angular/core';
import {LocalStorageService} from "../local-storage/local-storage.service";
import {DOCUMENT} from "@angular/common";

@Injectable({
    providedIn: 'root'
})
export class ThemeService {

    private readonly docClassList: DOMTokenList;
    private readonly themeKey: string = 'theme';
    private readonly darkTheme: string = 'dark-theme';
    private readonly lightTheme: string = 'light-theme';
    private oldTheme: string | null;

    constructor(@Inject(DOCUMENT) private readonly doc: Document, private readonly localStorage: LocalStorageService) {
        this.docClassList = doc.documentElement.classList;
    }

    public loadSavedTheme(): void{
        const savedTheme = this.localStorage.getItem(this.themeKey) as string;
        savedTheme && this.docClassList.add(savedTheme);
    }

    public toggleTheme() : void {
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

        // save theme
        this.localStorage.setItem(this.themeKey, newTheme);
        // TODO: save on server
    }


}
