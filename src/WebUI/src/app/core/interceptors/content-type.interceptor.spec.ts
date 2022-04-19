import { TestBed } from '@angular/core/testing';

import { ContentTypeInterceptor } from './content-type.interceptor';

describe('ContentTypeInterceptor', () => {
  beforeEach(() => TestBed.configureTestingModule({
    providers: [
      ContentTypeInterceptor
      ]
  }));

  it('should be created', () => {
    const interceptor: ContentTypeInterceptor = TestBed.inject(ContentTypeInterceptor);
    expect(interceptor).toBeTruthy();
  });
});
