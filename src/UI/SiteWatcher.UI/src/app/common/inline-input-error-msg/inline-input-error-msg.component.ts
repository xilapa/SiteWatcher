import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'sw-inline-input-error-msg',
  templateUrl: './inline-input-error-msg.component.html',
  styleUrls: ['./inline-input-error-msg.component.css']
})
export class InlineInputErrorMsgComponent implements OnInit {

  @Input() public message: string;
  @Input() public show_if: boolean | undefined = false;

  ngOnInit(): void {
  }

}
