import { ELanguage } from "../lang/language";
import { ETheme } from "../theme/theme";

export interface User {
    Id: string,
    Name: string,
    Email: string,
    EmailConfirmed: boolean,
    Language: ELanguage,
    ProfilePic: string | null,
    Theme: ETheme
}

export interface UpdateUser {
  name: string,
  email: string,
  language: ELanguage,
  theme: ETheme
}

export interface UpdateUserResult {
  Token : string,
  ConfirmationEmailSend: boolean
}

export interface RegisterUserResult {
    Token : string,
    ConfirmationEmailSend: boolean
}


