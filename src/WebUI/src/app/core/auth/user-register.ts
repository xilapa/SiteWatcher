import {ELanguage} from "../lang/language";
import {ETheme} from "../theme/theme";

export interface UserRegister {
  name: string,
  email: string,
  language: ELanguage,
  theme: ETheme
}
