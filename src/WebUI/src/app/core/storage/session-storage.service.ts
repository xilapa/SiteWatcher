import { Inject, Injectable } from "@angular/core";
import { StorageBaseService } from "./storage-base.service";
import { DOCUMENT } from "@angular/common";

@Injectable({
  providedIn: 'root'
})
export class SessionStorageService extends StorageBaseService {
    constructor(@Inject(DOCUMENT) doc: Document){
        super(doc.defaultView?.sessionStorage as Storage);
    }
}