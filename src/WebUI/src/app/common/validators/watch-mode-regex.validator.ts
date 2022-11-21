import { AbstractControl, ValidationErrors } from "@angular/forms";
import { EWatchMode } from "../../alerts/common/e-watch-mode";

export const watchModeRegexValidator = (control: AbstractControl) :  ValidationErrors | null => {
    const watchMode = (control.parent?.get('watchMode')?.value as EWatchMode);
    const isRegexWatch = watchMode == EWatchMode.Regex;

    if(!isRegexWatch)
        return null;

    if(!control.value)
        return { watchModeRegexValidator: true };

    if (control.value && control.value.trim().length == 0)
        return { watchModeRegexValidator: true };

    if(control.value.length > 512)
        return { watchModeRegexValidator: true };

    return null;
}


