import {Injectable} from '@angular/core';
import {ApiResponse} from "../../core/interfaces";
import {CreateAlertModel, DetailedAlertView} from "../common/alert";
import {Observable} from "rxjs";
import {HttpClient} from "@angular/common/http";
import {environment} from "../../../environments/environment";

@Injectable({
    providedIn: 'root'
})
export class AlertService {
    private readonly baseRoute = "alert";

    constructor(private readonly httpClient: HttpClient) {
    }

    public createAlert(createModel: CreateAlertModel): Observable<ApiResponse<DetailedAlertView>>{
        return this.httpClient
            .post<ApiResponse<DetailedAlertView>>(`${environment.baseApiUrl}/${this.baseRoute}`, createModel);
    }

}
