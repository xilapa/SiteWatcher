import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {CreateUpdateAlertComponent} from "./create-update-alert.component";
import {AuthGuard} from "../../core/guards/auth.guard";

const alertRoute: Routes = [
    {
        path: 'create',
        component: CreateUpdateAlertComponent,
        canActivate: [AuthGuard],
    },
    {
        path: 'update/:alertId',
        component: CreateUpdateAlertComponent,
        canActivate: [AuthGuard],
    }
];

@NgModule({
    imports: [RouterModule.forChild(alertRoute)],
    exports: [RouterModule]
})
export class CreateUpdateAlertRoutingModule {
}
