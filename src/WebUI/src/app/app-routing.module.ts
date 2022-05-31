import {NgModule} from '@angular/core';
import {PreloadAllModules, RouterModule, Routes} from '@angular/router';

const routes: Routes = [
    {path: '', pathMatch: 'full', redirectTo: 'home'},
    {path: 'home', loadChildren: () => import('./home/home.module').then(m => m.HomeModule)},
    {path: 'dash', loadChildren: () => import('./dashboard/dashboard.module').then(m => m.DashboardModule)},
    {path: 'settings', loadChildren: () => import('./settings/settings.module').then(m => m.SettingsModule)}
];

@NgModule({
    imports: [RouterModule.forRoot(routes, {useHash: true, preloadingStrategy: PreloadAllModules})],
    exports: [RouterModule]
})
export class AppRoutingModule {
}
