import { ELanguage } from '../enums/language';
import { DropdownOption } from './dropdown-option';

export const LanguageOptions: DropdownOption<number>[] = [
    {Display: "Brazilian Portuguese", Value: ELanguage.BrazilianPortuguese },
    {Display: "English", Value: ELanguage.English },
    {Display: "Spanish", Value: ELanguage.Spanish },
]