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

// Api response
export interface DetailedAlertViewApi {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency: AlertFrequency,
    LastVerification : Date | null,
    NotificationsSent: number,
    Site: SiteViewApi,
    WatchMode: DetailedWatchModeViewApi
}

export interface SiteViewApi {
    Name: string,
    Uri: string
}

export interface DetailedWatchModeViewApi {
    Id: string,
    WatchMode: EWatchMode,
    Term: string | null
}

export interface SimpleAlertViewApi {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency : AlertFrequency,
    LastVerification : Date | null,
    NotificationsSent: number,
    SiteName: string,
    WatchMode: EWatchMode
}


// front end interface
export interface DetailedAlertView {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency: AlertFrequency,
    LastVerification : Date | null,
    NotificationsSent: number,
    Site: SiteView,
    WatchMode: DetailedWatchModeView
}

export interface SiteView {
    Name: string,
    Uri?: string | undefined
}

export interface DetailedWatchModeView {
    Id?: string | undefined,
    WatchMode: EWatchMode,
    Term?: string | undefined
}


