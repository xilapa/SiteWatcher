import {Component, Input} from '@angular/core';

@Component({
  selector: 'sw-side-bar',
  templateUrl: './side-bar.component.html',
  styleUrls: ['./side-bar.component.css']
})
export class SideBarComponent {

    active = false;
    @Input() position : string;

    public toggle() : void {
        this.active = !this.active;
    }
}
