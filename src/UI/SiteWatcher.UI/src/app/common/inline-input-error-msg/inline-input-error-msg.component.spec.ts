import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InlineInputErrorMsgComponent } from './inline-input-error-msg.component';

describe('InlineInputErrorMsgComponent', () => {
  let component: InlineInputErrorMsgComponent;
  let fixture: ComponentFixture<InlineInputErrorMsgComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ InlineInputErrorMsgComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(InlineInputErrorMsgComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
