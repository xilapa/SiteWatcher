import { Component, OnInit } from '@angular/core';
import { UserService } from '../core/user/user.service';

@Component({
  selector: 'sw-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {

  constructor(private readonly userService : UserService) { }

  ngOnInit(): void {
    this.userService.getUser().subscribe(u => console.log(u));
  }

}
