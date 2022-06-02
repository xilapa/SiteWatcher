import { Component, OnInit } from '@angular/core';
import {DeviceService} from "../../../core/device/device.service";
import {Observable} from "rxjs";
import {UserService} from "../../../core/user/user.service";
import {User} from "../../../core/interfaces";

@Component({
  selector: 'sw-security-page',
  templateUrl: './security-page.component.html',
  styleUrls: ['./security-page.component.css']
})
export class SecurityPageComponent implements OnInit {

  mobileScreen$: Observable<Boolean>;
  user$: Observable<User | null>;

  constructor(private readonly deviceService: DeviceService,
              private readonly userService: UserService) { }

  ngOnInit(): void {
    this.mobileScreen$ = this.deviceService.isMobileScreen();
    this.user$ = this.userService.getUser();
  }

  logoutAllDevices() : void {
      this.userService.logoutAllDevices();
  }

}
