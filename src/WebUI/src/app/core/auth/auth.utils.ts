import { Router } from '@angular/router';
import { Data } from '../shared-data/shared-data';

export function checkAuthRedirect(href: string, router: Router) {
    const authParams = ['state', 'code', 'scope', 'authuser', 'prompt'];
    const url = new URL(href);
    const missingAuthParams = authParams.some(param => url.search.indexOf(param) == -1);
    if (missingAuthParams)
        return;

    Data.Share('authURL', url);
    router.navigateByUrl('/home/auth');
}