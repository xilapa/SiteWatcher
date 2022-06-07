import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReactivateAccountComponent } from './reactivate-account.component';

describe('ReactivateAccountComponent', () => {
  let component: ReactivateAccountComponent;
  let fixture: ComponentFixture<ReactivateAccountComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ ReactivateAccountComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(ReactivateAccountComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
