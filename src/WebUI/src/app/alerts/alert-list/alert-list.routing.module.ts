import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {AuthGuard} from "../../core/guards/auth.guard";
import {AlertListComponent} from "./alert-list.component";

const alertListRoute: Routes = [
    {
        path: '',
        component: AlertListComponent,
        canActivate: [AuthGuard],
    }
];

@NgModule({
    imports: [RouterModule.forChild(alertListRoute)],
    exports: [RouterModule]
})
export class AlertListRoutingModule {
}
