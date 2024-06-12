import { AbstractControl, ValidationErrors } from "@angular/forms";
import { Rules } from "../../alerts/common/e-rule-type";

export const termRuleValidator = (control: AbstractControl) :  ValidationErrors | null => {
    const rule = (control.parent?.get('rule')?.value as Rules);
    const isTermWatch = rule == Rules.Term;

    if(!isTermWatch)
        return null;

    if(!control.value)
        return { termRuleValidator: true };

    if (control.value && control.value.trim().length == 0)
        return { termRuleValidator: true };

    if(control.value.length < 3 || control.value.length > 64)
        return { termRuleValidator: true };

    return null;
}


