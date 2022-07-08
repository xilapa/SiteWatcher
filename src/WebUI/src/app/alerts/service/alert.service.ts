import {Injectable} from '@angular/core';
import {ApiResponse, PaginatedList} from "../../core/interfaces";
import {
    AlertDetailsApi,
    AlertUtils,
    CreateAlertModel, DetailedAlertView,
    DetailedAlertViewApi, SimpleAlertViewApi
} from "../common/alert";
import {map, Observable, of, tap} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../../environments/environment";
import {Data} from "../../core/shared-data/shared-data";
import {LangUtils} from "../../core/lang/lang.utils";

@Injectable({
    providedIn: 'root'
})
export class AlertService {
    private readonly baseRoute = "alert";
    private readonly userAlertsKey = "userAlerts";
    private allUserAlertsLoaded = false;
    private readonly currentLocale: string;

    constructor(private readonly httpClient: HttpClient, window: Window) {
        this.currentLocale = LangUtils.getCurrentLocale(window);
    }

    public createAlert(createModel: CreateAlertModel): Observable<ApiResponse<DetailedAlertViewApi>> {
        return this.httpClient
            .post<ApiResponse<DetailedAlertViewApi>>(`${environment.baseApiUrl}/${this.baseRoute}`, createModel)
            .pipe(tap(res => {
                // adding the created alert to memory if all alerts are loaded
                if (this.allUserAlertsLoaded) {
                    const newAlert = AlertUtils
                        .DetailedAlertViewApiToInternal(res.Result, this.currentLocale);
                    const alertsAlreadyLoaded = Data.Get(this.userAlertsKey) as DetailedAlertView[];
                    const updatedAlertList = alertsAlreadyLoaded ?
                        alertsAlreadyLoaded.concat(newAlert) : [newAlert];
                    Data.Share(this.userAlertsKey, updatedAlertList);
                }
            }));
    }

    // load the initial list of alerts
    public getUserAlerts(isMobile: boolean): Observable<DetailedAlertView[]> {
        const alerts = Data.Get(this.userAlertsKey) as DetailedAlertView[];

        // if there was alerts stored doesnt call the server
        if (alerts && alerts.length != 0)
            return of(alerts);

        return this.loadUserAlertsFromServer(isMobile)
            .pipe(map(res => {
                Data.Share(this.userAlertsKey, res.Results);
                this.allUserAlertsLoaded = res.Results.length == res.Total;
                return res.Results;
            }));
    }

    // load more user alerts when user scrolls and add it to the app memory
    public getMoreUserAlerts(isMobile: boolean): Observable<DetailedAlertView[]> {
        const alertsAlreadyLoaded = Data.Get(this.userAlertsKey) as DetailedAlertView[];
        if (this.allUserAlertsLoaded)
            return of(alertsAlreadyLoaded);

        const lastId = alertsAlreadyLoaded[alertsAlreadyLoaded.length - 1].Id;

        return this.loadUserAlertsFromServer(isMobile, lastId)
            .pipe(map(newAlerts => {
                const allAlerts = alertsAlreadyLoaded.concat(newAlerts.Results);
                Data.Share(this.userAlertsKey, allAlerts);
                this.allUserAlertsLoaded = allAlerts.length == newAlerts.Total;
                return allAlerts;
            }));
    }

    private loadUserAlertsFromServer(isMobile: boolean, lastId?: string | undefined): Observable<PaginatedList<DetailedAlertView>> {
        const query = `?Take=${isMobile ? 5 : 7}${lastId ? '&LastAlertId=' + lastId : ''}`;

        return this.httpClient
            .get<ApiResponse<PaginatedList<SimpleAlertViewApi>>>(`${environment.baseApiUrl}/${this.baseRoute}${query}`)
            .pipe(map(apiResponse => {

                // map simple alert to detailed alerts
                const detailedAlerts = apiResponse.Result.Results
                    .map(simpleAlert => AlertUtils
                        .SimpleAlertViewApiToInternal(simpleAlert, this.currentLocale));

                // map to paginated list of detailed alerts
                const paginatedDetailedAlerts: PaginatedList<DetailedAlertView> = {
                    Total: apiResponse.Result.Total,
                    Results: detailedAlerts
                }
                return paginatedDetailedAlerts;
            }))
    }

    public getAlertDetails(alert: DetailedAlertView): Observable<DetailedAlertView> {
        return this.httpClient.get<ApiResponse<AlertDetailsApi>>(`${environment.baseApiUrl}/${this.baseRoute}/${alert.Id}/details`)
            .pipe(map(apiResponse => {
                const populatedAlert =
                    AlertUtils.PopulateAlertDetails(apiResponse.Result, alert);

                // update the internal list
                let alertsLoaded = Data.Get(this.userAlertsKey) as DetailedAlertView[];
                const index = alertsLoaded.findIndex(a => a.Id == populatedAlert.Id);
                alertsLoaded[index] = populatedAlert;

                return populatedAlert;
            }));
    }
}
