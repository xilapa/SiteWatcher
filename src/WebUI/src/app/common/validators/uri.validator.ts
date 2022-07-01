import { AbstractControl, ValidationErrors } from "@angular/forms";

export const uriValidator = (control: AbstractControl) :  ValidationErrors | null => {
    if (control.value && control.value.trim())
    {
        let validUrl = true;

        try {
            new URL(control.value)
        } catch {
            validUrl = false;
        }

        return validUrl ? null : { invalidUrl: true };
    }
    return null;
}
