import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InlineInputErrorMsgComponent } from './inline-input-error-msg.component';



@NgModule({
  declarations: [InlineInputErrorMsgComponent],
  imports: [
    CommonModule
  ],
  exports:[InlineInputErrorMsgComponent]
})
export class InlineInputErrorMsgModule { }
