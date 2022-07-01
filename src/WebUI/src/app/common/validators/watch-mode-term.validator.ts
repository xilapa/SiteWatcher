import {AbstractControl, ValidationErrors} from "@angular/forms";
import {WatchMode} from "../../alerts/common/watch-mode";

export const watchModeTermValidator = (control: AbstractControl) :  ValidationErrors | null => {
    const watchMode = (control.parent?.get('watch-mode')?.value as WatchMode);
    const isTermWatch = watchMode == WatchMode.Term;

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


