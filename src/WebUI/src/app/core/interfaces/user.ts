import {ELanguage} from "../lang/language";
import {ETheme} from "../theme/theme";

export interface User {
    id: string,
    name: string,
    email: string,
    emailConfirmed: boolean,
    language: ELanguage,
    profilePic: string | null,
    theme: ETheme
}

export interface UpdateUser {
  name: string,
  email: string,
  language: ELanguage,
  theme: ETheme
}

export interface UpdateUserResult {
  token : string,
  ConfirmationEmailSend: boolean
}


