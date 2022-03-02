import { Component } from '@angular/core';
import { AuthService } from 'src/app/core/auth/auth.service';


@Component({
  selector: 'sw-log-in',
  templateUrl: './log-in.component.html',
  styleUrls: ['./log-in.component.css']
})
export class LogInComponent {
  
  constructor(private readonly authService: AuthService) { }
  
  // TODO: implementar returnUrl

  public login() {
    this.authService.googleLogin();
  }

  public register() {
    this.authService.googleRegister();
  }
}
