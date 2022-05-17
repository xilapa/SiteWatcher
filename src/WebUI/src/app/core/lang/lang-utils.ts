import {ELanguage, LanguageFileName} from "../enums";

export class LangUtils {

    public static getLangFileName(lang: ELanguage): string {
        let fileName;
        switch (lang) {
            case ELanguage.BrazilianPortuguese:
                fileName = LanguageFileName.BrazilianPortuguese;
                break;
            case ELanguage.Spanish:
                fileName = LanguageFileName.Spanish;
                break;
            default:
                fileName = LanguageFileName.English;
                break;
        }
        return fileName;
    }
}
