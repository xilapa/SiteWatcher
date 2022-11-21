import { AlertFrequencyUtils, EAlertFrequency } from "./e-alert-frequency";
import { EWatchMode, WatchModeUtils } from "./e-watch-mode";

export interface CreateUpdateAlertModel {
    name: string,
    frequency: EAlertFrequency,
    siteName: string,
    siteUri: string,
    watchMode: EWatchMode,
    term: string | undefined,
    regexPattern: string | undefined,
    notifyOnDisappearance: boolean | undefined
}

// Api response
export interface DetailedAlertViewApi {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency: EAlertFrequency,
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
    WatchMode: EWatchMode,
    Term: string | undefined,
    RegexPattern: string | undefined,
    NotifyOnDisappearance: boolean | undefined
}

export interface SimpleAlertViewApi {
    Id: string,
    Name: string,
    CreatedAt: Date,
    Frequency: EAlertFrequency,
    LastVerification: Date | undefined,
    NotificationsSent: number,
    SiteName: string,
    WatchMode: EWatchMode
}

export interface AlertDetailsApi {
    Id: string,
    SiteUri: string,
    WatchModeId: string,
    Term?: string,
    RegexPattern?: string,
    NotifyOnDisappearance?: boolean
}

// Api input
export interface UpdateInfo<T> {
    NewValue?: T
}

export interface UpdateAlertData {
    AlertId: string,
    Name?: UpdateInfo<string>,
    Frequency?: UpdateInfo<EAlertFrequency>,
    SiteName?: UpdateInfo<string>,
    SiteUri?: UpdateInfo<string>,
    WatchMode?: UpdateInfo<EWatchMode>,
    Term?: UpdateInfo<string>,
    RegexPattern?: UpdateInfo<string>,
    NotifyOnDisappearance?: UpdateInfo<boolean>,
}


// front end interface
export interface DetailedAlertView {
    Id?: string,
    Name: string,
    CreatedAt?: Date,
    Frequency?: EAlertFrequency,
    LastVerification?: Date,
    NotificationsSent?: number,
    Site: SiteView,
    WatchMode: DetailedWatchModeView,
    FullyLoaded?: boolean,
    FrequencyTranslationKey?: string,
    WatchModeTranslationKey?: string,
    LocalizedDateString?: string,
    NotifyOnDisappearanceTranslationKey?: string
}

export interface SiteView {
    Name: string,
    Uri?: string | undefined
}

export interface DetailedWatchModeView {
    Id?: string | undefined,
    WatchMode: EWatchMode,
    Term?: string | undefined,
    RegexPattern?: string | undefined,
    NotifyOnDisappearance?: boolean | undefined
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
                + createdAt.toLocaleTimeString(currentLocale),
            NotifyOnDisappearanceTranslationKey: WatchModeUtils.getNotifyOnDisappearanceTranslationKey(apiView.WatchMode.NotifyOnDisappearance)
        }
    }

    public static SimpleAlertViewApiToInternal(simpleApiView: SimpleAlertViewApi, currentLocale: string): DetailedAlertView {
        const createdAt = new Date(simpleApiView.CreatedAt);
        return {
            Id: simpleApiView.Id,
            Name: simpleApiView.Name,
            CreatedAt: createdAt,
            Frequency: simpleApiView.Frequency,
            LastVerification: simpleApiView.LastVerification ? new Date(simpleApiView.LastVerification) : undefined,
            NotificationsSent: simpleApiView.NotificationsSent,
            Site: { Name: simpleApiView.SiteName },
            WatchMode: { WatchMode: simpleApiView.WatchMode },
            FullyLoaded: false,
            FrequencyTranslationKey: AlertFrequencyUtils.getFrequencyTranslationKey(simpleApiView.Frequency),
            WatchModeTranslationKey: WatchModeUtils.getWatchModeTranslationKey(simpleApiView.WatchMode),
            LocalizedDateString: createdAt.toLocaleDateString(currentLocale) + ' '
                + createdAt.toLocaleTimeString(currentLocale)
        }
    }

    public static PopulateAlertDetails(apiDetails: AlertDetailsApi, detailedAlert: DetailedAlertView): DetailedAlertView {
        detailedAlert.Site.Uri = apiDetails.SiteUri;
        detailedAlert.WatchMode.Id = apiDetails.WatchModeId;
        detailedAlert.WatchMode.Term = apiDetails.Term;
        detailedAlert.WatchMode.RegexPattern = apiDetails.RegexPattern;
        detailedAlert.WatchMode.NotifyOnDisappearance = apiDetails.NotifyOnDisappearance;
        detailedAlert.NotifyOnDisappearanceTranslationKey = WatchModeUtils.getNotifyOnDisappearanceTranslationKey(apiDetails.NotifyOnDisappearance)
        detailedAlert.FullyLoaded = true;
        return detailedAlert;
    }

    public static GetUpdateAlertData(rawValues: CreateUpdateAlertModel, initialValues: DetailedAlertView): UpdateAlertData {
        let updateAlertData: UpdateAlertData = { AlertId: initialValues.Id as string };

        if (rawValues.name != initialValues.Name)
            updateAlertData.Name = { NewValue: rawValues.name }

        if (rawValues.frequency != initialValues.Frequency)
            updateAlertData.Frequency = { NewValue: rawValues.frequency }

        if (rawValues.siteName != initialValues.Site.Name)
            updateAlertData.SiteName = { NewValue: rawValues.siteName }

        if (rawValues.siteUri != initialValues.Site.Uri)
            updateAlertData.SiteUri = { NewValue: rawValues.siteUri }

        if (rawValues.watchMode != initialValues.WatchMode.WatchMode)
            updateAlertData.WatchMode = { NewValue: rawValues.watchMode }

        if (rawValues.term != initialValues.WatchMode.Term && rawValues.watchMode == EWatchMode.Term)
            updateAlertData.Term = { NewValue: rawValues.term }

        if (rawValues.regexPattern != initialValues.WatchMode.RegexPattern && rawValues.watchMode == EWatchMode.Regex)
            updateAlertData.RegexPattern = { NewValue: rawValues.regexPattern }

        if (rawValues.notifyOnDisappearance != initialValues.WatchMode.NotifyOnDisappearance && rawValues.watchMode == EWatchMode.Regex)
            updateAlertData.NotifyOnDisappearance = { NewValue: rawValues.notifyOnDisappearance }

        return updateAlertData;
    }
}


