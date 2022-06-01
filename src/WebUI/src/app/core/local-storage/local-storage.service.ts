import {Inject, Injectable} from '@angular/core';
import {DOCUMENT} from "@angular/common";

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService {

    private storage: Storage;

    constructor(@Inject(DOCUMENT) doc: Document){
        this.storage = doc.defaultView?.localStorage as Storage;
    }

    public setItem = (key: string, value: any): void =>
        this.storage.setItem(key, value);

    public getItem = (key: string): any =>
        this.storage.getItem(key);

    public removeItem = (key: string): void =>
        this.storage.removeItem(key);

    public getAndRemove = (key: string): any => {
        const value = this.getItem(key);
        this.removeItem(key);
        return value;
    }
}
