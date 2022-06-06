import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {ConfirmEmailComponent} from "./confirm-email/confirm-email.component";


const externalRoutes: Routes = [
    { path: '', component: ConfirmEmailComponent},
];

@NgModule({
    imports: [RouterModule.forChild(externalRoutes)],
    exports: [RouterModule]
})
export class ExternalRoutesRoutingModule {
}
