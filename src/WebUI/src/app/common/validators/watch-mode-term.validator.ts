import {AbstractControl, ValidationErrors} from "@angular/forms";
import {EWatchMode} from "../../alerts/common/e-watch-mode";

export const watchModeTermValidator = (control: AbstractControl) :  ValidationErrors | null => {
    const watchMode = (control.parent?.get('watchMode')?.value as EWatchMode);
    const isTermWatch = watchMode == EWatchMode.Term;

    if(!isTermWatch)
        return null;

    if(!control.value)
        return { watchModeTermValidator: true };

    if (control.value && control.value.trim().length == 0)
        return { watchModeTermValidator: true };

    if(control.value.length < 3 || control.value.length > 64)
        return { watchModeTermValidator: true };

    return null;
}


