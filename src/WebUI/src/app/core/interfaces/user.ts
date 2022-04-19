import { ELanguage } from "../enums";

export interface User {
    id: string,
    name: string,
    email: string,
    emailConfirmed: boolean,
    language: ELanguage
}