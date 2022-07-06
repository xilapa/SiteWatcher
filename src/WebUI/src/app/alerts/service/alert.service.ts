import {Injectable} from '@angular/core';
import {ApiResponse} from "../../core/interfaces";
import {
    CreateAlertModel, DetailedAlertView,
    DetailedAlertViewApi, SimpleAlertViewApi
} from "../common/alert";
import {map, Observable, of, switchMap} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../../environments/environment";
import {Data} from "../../core/shared-data/shared-data";

@Injectable({
    providedIn: 'root'
})
export class AlertService {
    private readonly baseRoute = "alert";
    private readonly userAlertsKey = "userAlerts";

    constructor(private readonly httpClient: HttpClient) {
    }

    public createAlert(createModel: CreateAlertModel): Observable<ApiResponse<DetailedAlertViewApi>>{
        return this.httpClient
            .post<ApiResponse<DetailedAlertViewApi>>(`${environment.baseApiUrl}/${this.baseRoute}`, createModel);
    }

    public getUserAlerts() : Observable<DetailedAlertView[]> {
        const alerts = Data.Get(this.userAlertsKey) as DetailedAlertView[];

        // if there was alerts stored, doesnt call the server
        if(alerts && alerts.length != 0)
            return of(alerts);

        return this.httpClient.get<ApiResponse<SimpleAlertViewApi[]>>(`${environment.baseApiUrl}/${this.baseRoute}`)
            .pipe(map(apiResponse => {
                const detailedAlerts = apiResponse.Result.map(simpleAlert => {
                    const detailedAlert: DetailedAlertView = {
                        Id: simpleAlert.Id,
                        Name: simpleAlert.Name,
                        CreatedAt: new Date(simpleAlert.CreatedAt),
                        Frequency: simpleAlert.Frequency,
                        LastVerification: simpleAlert.LastVerification ? new Date(simpleAlert.LastVerification) : null,
                        NotificationsSent: simpleAlert.NotificationsSent,
                        Site: {Name: simpleAlert.SiteName},
                        WatchMode: {WatchMode: simpleAlert.WatchMode}
                    }
                    return detailedAlert;
                });
                Data.Share(this.userAlertsKey, detailedAlerts);
                return detailedAlerts;
            }))
    }

}
