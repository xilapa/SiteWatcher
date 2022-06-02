import {ELanguage} from "../lang/language";
import {ETheme} from "../theme/theme";
// TODO: mover para arquivo do user
export interface UserRegister {
  name: string,
  email: string,
  language: ELanguage,
  theme: ETheme
}
