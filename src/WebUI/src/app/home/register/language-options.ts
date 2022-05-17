import {DropdownOption} from "../../core/interfaces/dropdown-option";
import {ELanguage} from "../../core/lang/language";

export const LanguageOptions: DropdownOption<number>[] = [
    {Display: "Brazilian Portuguese", Value: ELanguage.BrazilianPortuguese},
    {Display: "English", Value: ELanguage.English},
    {Display: "Spanish", Value: ELanguage.Spanish},
]
