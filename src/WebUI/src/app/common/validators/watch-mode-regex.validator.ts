import { AbstractControl, ValidationErrors } from "@angular/forms";
import { Rules } from "../../alerts/common/e-watch-mode";

export const regexRuleValidator = (control: AbstractControl): ValidationErrors | null => {
    const rule = (control.parent?.get('rule')?.value as Rules);
    const isRegexWatch = rule == Rules.Regex;

    if (!isRegexWatch)
        return null;

    if (!control.value)
        return { regexRuleValidator: true };

    const trimmedValue = control.value.trim() as string;

    if (trimmedValue.length == 0)
        return { regexRuleValidator: true };

    if (trimmedValue[0] == '/')
        return { regexRuleValidator: true };

    const invalidEndRegex = new RegExp('\/[gmixsuajd]*$') 
    if (invalidEndRegex.test(trimmedValue))
        return { regexRuleValidator: true };

    if (control.value.length > 512)
        return { regexRuleValidator: true };

    return null;
}
