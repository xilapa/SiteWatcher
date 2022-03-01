import { DOCUMENT } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { Router } from '@angular/router';
import { checkAuthRedirect } from './core/auth/auth.utils';

@Component({
  selector: 'sw-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'SiteWatcher.UI';

  constructor(@Inject(DOCUMENT) doc: Document, router: Router) {
    checkAuthRedirect(doc.location.href, router);
  }
}