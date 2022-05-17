import {ELanguage, LanguageFileName} from "./language";

export class LangUtils {

    public static getLangFileName(lang: ELanguage): string {
        switch (lang) {
            case ELanguage.BrazilianPortuguese:
                return LanguageFileName.BrazilianPortuguese;
            case ELanguage.Spanish:
                return LanguageFileName.Spanish;
            default:
                return LanguageFileName.English;
        }
    }

    public static getCurrentBrowserLanguageFileName(window: Window): string {
        const langs = window.navigator.languages;

        let fileName = LanguageFileName.English;
        let found = false;
        langs.find(l => {
            const _lang = l.substring(0, 2);

            switch (_lang) {
                case "pt":
                    fileName = LanguageFileName.BrazilianPortuguese;
                    found = true;
                    break;
                case "es":
                    fileName = LanguageFileName.Spanish;
                    found = true;
                    break;
                case "en":
                    fileName = LanguageFileName.English;
                    found = true;
                    break;
            }
            return found;
        });
        return fileName;
    }
}
