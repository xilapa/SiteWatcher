import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BasePageLayoutComponent } from './base-page-layout.component';

describe('BasePageLayoutComponent', () => {
  let component: BasePageLayoutComponent;
  let fixture: ComponentFixture<BasePageLayoutComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ BasePageLayoutComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(BasePageLayoutComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
