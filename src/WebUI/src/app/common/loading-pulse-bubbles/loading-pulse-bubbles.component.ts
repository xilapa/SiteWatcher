import {Component, Input} from '@angular/core';

@Component({
  selector: 'sw-loading-pulse-bubbles',
  templateUrl: './loading-pulse-bubbles.component.html',
  styleUrls: ['./loading-pulse-bubbles.component.css']
})
export class LoadingPulseBubblesComponent  {

    @Input() loadindText: string;
}
