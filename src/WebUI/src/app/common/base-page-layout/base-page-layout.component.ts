import {Component, Input, OnInit} from '@angular/core';

@Component({
    selector: 'sw-base-page-layout',
    templateUrl: './base-page-layout.component.html',
    styleUrls: ['./base-page-layout.component.css']
})
export class BasePageLayoutComponent implements OnInit {

    @Input() pageTitletranslationKey: string;
    constructor() {
    }

    ngOnInit(): void {
    }

}
