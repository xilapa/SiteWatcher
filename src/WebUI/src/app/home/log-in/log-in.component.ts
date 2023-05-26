import { Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from "../../core/auth/service/auth.service";
import { UserService } from "../../core/user/user.service";


@Component({
  selector: 'sw-log-in',
  templateUrl: './log-in.component.html',
  styleUrls: ['./log-in.component.css']
})
export class LogInComponent {

  private returnUrl: string | null;

  constructor(
    private readonly authService: AuthService,
    private readonly activatedRoute: ActivatedRoute,
    private readonly userService: UserService
  ) {
  }

  public async login() {
    this.setReturnUrl();
    await this.authService.googleLogin();
  }

  private setReturnUrl() : void {
    this.returnUrl = this.activatedRoute.snapshot.queryParams?.["returnUrl"] ?? null;
    this.returnUrl && this.userService.setReturnUrl(this.returnUrl);
  }
}
