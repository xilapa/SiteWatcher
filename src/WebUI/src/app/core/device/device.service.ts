import {Injectable, OnDestroy} from '@angular/core';
import {BehaviorSubject, debounceTime, fromEvent, Observable, Subscription, tap} from "rxjs";
import {MediaMatcher} from "@angular/cdk/layout";

@Injectable({
    providedIn: 'root'
})
export class DeviceService implements OnDestroy {

    private hasTouch = new BehaviorSubject<boolean>(false);
    private mobileScreen = new BehaviorSubject<boolean>(false);
    private resizeEventSub: Subscription;

    constructor(private readonly media: MediaMatcher, private window: Window) {
        const _hasTouch = media.matchMedia('(hover: none)').matches;
        this.hasTouch.next(_hasTouch ?? false);

        this.mobileScreen.next(this.checkMobileScreen());
        this.resizeEventSub = fromEvent(window, 'resize')
            .pipe(
                debounceTime(500),
                tap(() => this.mobileScreen.next(this.checkMobileScreen())))
            .subscribe();
    }

    ngOnDestroy(): void {
        this.resizeEventSub?.unsubscribe();
    }

    isMobileScreen(): Observable<boolean> {
        return this.mobileScreen.asObservable();
    }

    hasTouchScreen(): Observable<boolean> {
        return this.hasTouch.asObservable();
    }

    private checkMobileScreen() : boolean {
        return this.media.matchMedia('only screen and (max-width: 600px)').matches;
    }
}
