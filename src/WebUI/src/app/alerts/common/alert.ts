import { AlertFrequencyUtils, EAlertFrequency } from "./e-alert-frequency";
import { RuleUtils, Rules } from "./e-rule-type";

export interface CreateUpdateAlertModel {
    name: string,
    frequency: EAlertFrequency,
    siteName: string,
    siteUri: string,
    rule: Rules,
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
    TriggeringsCount: number,
    Site: SiteViewApi,
    Rule: DetailedRuleViewApi
}

export interface SiteViewApi {
    Name: string,
    Uri: string
}

export interface DetailedRuleViewApi {
    Rule: Rules,
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
    TriggeringsCount: number,
    SiteName: string,
    Rule: Rules
}

export interface AlertDetailsApi {
    Id: string,
    SiteUri: string,
    RuleId: string,
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
    Rule?: UpdateInfo<Rules>,
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
    TriggeringsCount?: number,
    Site: SiteView,
    Rule: DetailedRuleView,
    FullyLoaded?: boolean,
    FrequencyTranslationKey?: string,
    RuleTranslationKey?: string,
    LocalizedDateString?: string,
    NotifyOnDisappearanceTranslationKey?: string
}

export interface SiteView {
    Name: string,
    Uri?: string | undefined
}

export interface DetailedRuleView {
    Id?: string | undefined,
    Rule: Rules,
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
            TriggeringsCount: apiView.TriggeringsCount,
            Site: apiView.Site,
            Rule: apiView.Rule,
            FullyLoaded: true,
            FrequencyTranslationKey: AlertFrequencyUtils.getFrequencyTranslationKey(apiView.Frequency),
            RuleTranslationKey: RuleUtils.getRuleTranslationKey(apiView.Rule.Rule),
            LocalizedDateString: createdAt.toLocaleDateString(currentLocale) + ' '
                + createdAt.toLocaleTimeString(currentLocale),
            NotifyOnDisappearanceTranslationKey: RuleUtils.getNotifyOnDisappearanceTranslationKey(apiView.Rule.NotifyOnDisappearance)
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
            TriggeringsCount: simpleApiView.TriggeringsCount,
            Site: { Name: simpleApiView.SiteName },
            Rule: { Rule: simpleApiView.Rule },
            FullyLoaded: false,
            FrequencyTranslationKey: AlertFrequencyUtils.getFrequencyTranslationKey(simpleApiView.Frequency),
            RuleTranslationKey: RuleUtils.getRuleTranslationKey(simpleApiView.Rule),
            LocalizedDateString: createdAt.toLocaleDateString(currentLocale) + ' '
                + createdAt.toLocaleTimeString(currentLocale)
        }
    }

    public static PopulateAlertDetails(apiDetails: AlertDetailsApi, detailedAlert: DetailedAlertView): DetailedAlertView {
        detailedAlert.Site.Uri = apiDetails.SiteUri;
        detailedAlert.Rule.Id = apiDetails.RuleId;
        detailedAlert.Rule.Term = apiDetails.Term;
        detailedAlert.Rule.RegexPattern = apiDetails.RegexPattern;
        detailedAlert.Rule.NotifyOnDisappearance = apiDetails.NotifyOnDisappearance;
        detailedAlert.NotifyOnDisappearanceTranslationKey = RuleUtils.getNotifyOnDisappearanceTranslationKey(apiDetails.NotifyOnDisappearance)
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

        if (rawValues.rule != initialValues.Rule.Rule)
            updateAlertData.Rule = { NewValue: rawValues.rule }

        if (rawValues.term != initialValues.Rule.Term && rawValues.rule == Rules.Term)
            updateAlertData.Term = { NewValue: rawValues.term }

        // includes all fields of a rule
        if (rawValues.rule == Rules.Regex)
        {
            updateAlertData.NotifyOnDisappearance = { NewValue: rawValues.notifyOnDisappearance }

            if(rawValues.regexPattern != initialValues.Rule.RegexPattern)
                updateAlertData.RegexPattern = { NewValue: rawValues.regexPattern }
        }
        return updateAlertData;
    }
}


