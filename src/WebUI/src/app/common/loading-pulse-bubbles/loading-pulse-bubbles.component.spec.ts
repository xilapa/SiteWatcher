import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoadingPulseBubblesComponent } from './loading-pulse-bubbles.component';

describe('LoadingPulseBubblesComponent', () => {
  let component: LoadingPulseBubblesComponent;
  let fixture: ComponentFixture<LoadingPulseBubblesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ LoadingPulseBubblesComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(LoadingPulseBubblesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
