import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ConfirmEmailComponent} from "./confirm-email/confirm-email.component";
import {ReactivateAccountComponent} from "./reactivate-account/reactivate-account.component";


const externalRoutes: Routes = [
    { path: 'confirm-email', component: ConfirmEmailComponent},
    { path: 'reactivate-account', component: ReactivateAccountComponent},
];

@NgModule({
    imports: [RouterModule.forChild(externalRoutes)],
    exports: [RouterModule]
})
export class ExternalRoutesRoutingModule {
}
