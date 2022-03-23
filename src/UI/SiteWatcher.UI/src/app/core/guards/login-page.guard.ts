import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router, RouterStateSnapshot, UrlTree } from '@angular/router';
import { map, Observable } from 'rxjs';
import { UserService } from '../user/user.service';

@Injectable({
  providedIn: 'root'
})
export class LoginPageGuard implements CanActivate {

  constructor(
    private readonly userService: UserService,
    private readonly router: Router
    ) { }

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
      return this.userService.getUser()
                        .pipe(map(user => {
                          if(user) {
                            this.router.navigate(['dash'])
                            return false;
                          }

                          return true;
                        }));
  }
  
}
