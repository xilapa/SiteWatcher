import {AlertFrequency, AlertFrequencyUtils} from "./alert-frequency";
import {EWatchMode, WatchModeUtils} from "./e-watch-mode";

export interface CreateAlertModel {
    name: string,
    frequency: AlertFrequency,
    siteName: string,
    siteUri: string,
    watchMode: EWatchMode,
    term: string | undefined
}

// Api response
export interface DetailedAlertViewApi {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency: AlertFrequency,
    LastVerification: Date | undefined,
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
    Term: string | undefined
}

export interface SimpleAlertViewApi {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency: AlertFrequency,
    LastVerification: Date | undefined,
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
    LastVerification: Date | undefined,
    NotificationsSent: number,
    Site: SiteView,
    WatchMode: DetailedWatchModeView,
    FullyLoaded: boolean,
    FrequencyTranslationKey? : string,
    WatchModeTranslationKey? : string,
    LocalizedDateString? : string
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


export class AlertUtils {

    public static DetailedAlertViewApiToInternal(apiView: DetailedAlertViewApi, currentLocale: string): DetailedAlertView {
        const createdAt = new Date(apiView.CreatedAt);
        return {
            Id: apiView.Id,
            Name: apiView.Name,
            CreatedAt: createdAt,
            Frequency: apiView.Frequency,
            LastVerification: apiView.LastVerification ? new Date(apiView.LastVerification) : undefined,
            NotificationsSent: apiView.NotificationsSent,
            Site: apiView.Site,
            WatchMode: apiView.WatchMode,
            FullyLoaded: true,
            FrequencyTranslationKey: AlertFrequencyUtils.getFrequencyTranslationKey(apiView.Frequency),
            WatchModeTranslationKey: WatchModeUtils.getWatchModeTranslationKey(apiView.WatchMode.WatchMode),
            LocalizedDateString: createdAt.toLocaleDateString(currentLocale) + ' '
                + createdAt.toLocaleTimeString(currentLocale)
        }
    }

    public static SimpleAlertViewApiToInternal(simpleApiView : SimpleAlertViewApi, currentLocale: string): DetailedAlertView{
        const createdAt = new Date(simpleApiView.CreatedAt);
        return {
            Id: simpleApiView.Id,
            Name: simpleApiView.Name,
            CreatedAt: createdAt,
            Frequency: simpleApiView.Frequency,
            LastVerification: simpleApiView.LastVerification ? new Date(simpleApiView.LastVerification) : undefined,
            NotificationsSent: simpleApiView.NotificationsSent,
            Site: {Name: simpleApiView.SiteName},
            WatchMode: {WatchMode: simpleApiView.WatchMode},
            FullyLoaded: false,
            FrequencyTranslationKey: AlertFrequencyUtils.getFrequencyTranslationKey(simpleApiView.Frequency),
            WatchModeTranslationKey: WatchModeUtils.getWatchModeTranslationKey(simpleApiView.WatchMode),
            LocalizedDateString: createdAt.toLocaleDateString(currentLocale) + ' '
                + createdAt.toLocaleTimeString(currentLocale)
        }
    }
}


