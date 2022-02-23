import { Component } from '@angular/core';
import { faGoogle } from '@fortawesome/free-brands-svg-icons';
import { AuthService } from 'src/app/core/auth/auth.service';


@Component({
  selector: 'sw-log-in',
  templateUrl: './log-in.component.html',
  styleUrls: ['./log-in.component.css']
})
export class LogInComponent {

  public faGoogle = faGoogle;
  
  constructor(private readonly authService: AuthService) { }
  
  // TODO: implementar returnUrl

  public login() {
    this.authService.googleLogin();
  }

  public register() {
    this.authService.googleRegister();
  }
}
