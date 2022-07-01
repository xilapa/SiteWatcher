import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateUpdateAlertComponent } from './create-update-alert.component';

describe('CreateUpdateAlertComponent', () => {
  let component: CreateUpdateAlertComponent;
  let fixture: ComponentFixture<CreateUpdateAlertComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CreateUpdateAlertComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CreateUpdateAlertComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
