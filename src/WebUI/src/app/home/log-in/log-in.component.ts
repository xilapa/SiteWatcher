import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from 'src/app/core/auth/auth.service';


@Component({
  selector: 'sw-log-in',
  templateUrl: './log-in.component.html',
  styleUrls: ['./log-in.component.css']
})
export class LogInComponent implements OnInit {

  private returnUrl : string | null;

  constructor(
    private readonly authService: AuthService,
    private readonly activatedRoute : ActivatedRoute
    ) { }


  ngOnInit(): void {
    this.returnUrl = this.activatedRoute.snapshot.queryParams?.["returnUrl"] ?? null;
  }

  public login() {
    this.authService.googleLogin(this.returnUrl);
  }

  public register() {
    this.authService.googleRegister(this.returnUrl);
  }
}
