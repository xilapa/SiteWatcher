import { AbstractControl, ValidationErrors } from "@angular/forms";

export const invalidCharactersValidator = (control: AbstractControl) :  ValidationErrors | null => {
    if (control.value && control.value.trim() && !/^[A-Za-z ]*$/.test(control.value))
        return { invalidCharacters: true };
    return null;
}
