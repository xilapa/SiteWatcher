import {ELanguage} from "../lang/language";

export interface User {
    id: string,
    name: string,
    email: string,
    emailConfirmed: boolean,
    language: ELanguage,
    profilePic: string | null
}
