import { ELanguage as Language } from "../../lang/language";
import { ETheme as Theme } from "../../theme/theme";
import { EAuthTask as AuthTask } from "./auth-task";

export interface Session {
    Task: AuthTask,
    UserId: string,
    SessionId: string,
    Name: string,
    Email: string,
    EmailConfirmed: boolean,
    Language : Language,
    ProfilePicUrl: string | null,
    Theme: Theme
}

