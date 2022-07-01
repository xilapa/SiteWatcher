import {AlertFrequency} from "./alert-frequency";
import {EWatchMode} from "./e-watch-mode";

export interface CreateAlertModel {
    name: string,
    frequency: AlertFrequency,
    siteName: string,
    siteUri: string,
    watchMode: EWatchMode,
    term: string | null
}

export interface DetailedAlertView {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency: AlertFrequency,
    LastVerification : Date | null,
    NotificationsSent: number,
    Site: SiteView,
    WacthMode: DetailedWatchModeView
}

export interface SiteView {
    Name: string,
    Uri: string
}

export interface DetailedWatchModeView {
    Id: string,
    WatchMode: EWatchMode,
    Term: string | null
}
