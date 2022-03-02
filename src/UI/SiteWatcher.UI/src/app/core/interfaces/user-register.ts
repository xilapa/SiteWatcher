import { ELanguage } from "../enums";

export interface UserRegister{
    googleId: string,
    name: string,
    email: string,
    language: ELanguage
}