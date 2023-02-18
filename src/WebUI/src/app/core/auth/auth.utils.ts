import { Router } from '@angular/router';
import { Data } from '../shared-data/shared-data';

export class AuthUtils {

    public static checkAuthAndRedirect(href: string, router: Router) {
        const url = new URL(href);
        const missingAuthParams = url.search.indexOf('token') == -1;
        if (missingAuthParams)
            return;

        Data.Share('authURL', url);
        router.navigateByUrl('/home/auth');
    }
}
