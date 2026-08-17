import {Inject, Injectable} from '@angular/core';
import {DOCUMENT} from "@angular/common";
import { StorageBaseService } from './storage-base.service';

@Injectable({
  providedIn: 'root'
})
export class LocalStorageService extends StorageBaseService {
    constructor(@Inject(DOCUMENT) doc: Document){
        super(doc.defaultView?.localStorage as Storage);
    }
}
