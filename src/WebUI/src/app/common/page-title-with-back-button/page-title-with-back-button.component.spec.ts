import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageTitleWithBackButtonComponent } from './page-title-with-back-button.component';

describe('PageTitleWithBackButtonComponent', () => {
  let component: PageTitleWithBackButtonComponent;
  let fixture: ComponentFixture<PageTitleWithBackButtonComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ PageTitleWithBackButtonComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PageTitleWithBackButtonComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
