import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SecurityPageComponent } from './security-page.component';

describe('SecurityPageComponent', () => {
  let component: SecurityPageComponent;
  let fixture: ComponentFixture<SecurityPageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ SecurityPageComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(SecurityPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
