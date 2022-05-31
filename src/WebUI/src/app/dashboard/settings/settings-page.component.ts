import {AfterViewChecked, Component, ElementRef, OnInit, Renderer2, ViewChild} from '@angular/core';
import {Location} from '@angular/common'
import {DeviceService} from "../../core/device/device.service";
import {Observable} from "rxjs";
import {Router} from "@angular/router";

@Component({
  selector: 'sw-settings-page',
  templateUrl: './settings-page.component.html',
  styleUrls: ['./settings-page.component.css']
})
export class SettingsPageComponent implements OnInit, AfterViewChecked {

  mobileScreen$: Observable<boolean>;
  @ViewChild('profile') profileIcon: ElementRef;
  @ViewChild('security') securityIcon: ElementRef;
  private activePage: string;

  constructor(private readonly location: Location,
              private readonly deviceService: DeviceService,
              private readonly router: Router,
              private readonly renderer: Renderer2) {
  }

  ngOnInit(): void {
    this.mobileScreen$ = this.deviceService.isMobileScreen();
    this.activePage = 'profile';
  }

  ngAfterViewChecked() {
    this.toggleActiveClass(this.profileIcon, this.activePage === 'profile');
    this.toggleActiveClass(this.securityIcon, this.activePage === 'security');
  }

  goTo(url: string): void {
    this.router.navigateByUrl(`/dash/settings/${url}`);
    this.activePage = url;
  }

  private toggleActiveClass(element: ElementRef, active: boolean): void {
    if (!element) return;
    active ? this.renderer.addClass(element.nativeElement, 'active')
      : this.renderer.removeClass(element.nativeElement, 'active');
  }

}
