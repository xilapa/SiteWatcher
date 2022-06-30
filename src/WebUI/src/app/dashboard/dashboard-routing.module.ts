import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {AuthGuard} from '../core/guards/auth.guard';
import {DashboardComponent} from './dashboard.component';


const dashRoutes: Routes = [
    {
        path: '',
        component: DashboardComponent,
        canActivate: [AuthGuard],
        children:
            [
                {
                    path: 'settings',
                    loadChildren: () => import('./settings/settings.module').then(m => m.SettingsModule)
                },
                {
                    path: 'alert/create',
                    loadChildren: () => import('../alerts/create-update-alert/create-update-alert.module').then(m => m.CreateUpdateAlertModule)
                }
            ]
    },
];

@NgModule({
    imports: [RouterModule.forChild(dashRoutes)],
    exports: [RouterModule]
})
export class DashboardRoutingModule {
}
