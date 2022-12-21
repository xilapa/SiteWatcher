import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ConfirmEmailComponent } from "./confirm-email/confirm-email.component";
import { PrivacyPolicyComponent } from './privacy-policy/privacy-policy.component';
import { ReactivateAccountComponent } from "./reactivate-account/reactivate-account.component";


const externalRoutes: Routes = [
    { path: 'confirm-email', component: ConfirmEmailComponent},
    { path: 'reactivate-account', component: ReactivateAccountComponent},
    { path: 'privacy', component: PrivacyPolicyComponent},
];

@NgModule({
    imports: [RouterModule.forChild(externalRoutes)],
    exports: [RouterModule]
})
export class ExternalRoutesRoutingModule {
}
