import {Component, Input} from '@angular/core';

@Component({
  selector: 'sw-page-title-with-back-button',
  templateUrl: './page-title-with-back-button.component.html',
  styleUrls: ['./page-title-with-back-button.component.css']
})
export class PageTitleWithBackButtonComponent {
    @Input() translationKey: string;
    @Input() backRoute = '/dash';
}
