import {EAuthTask} from "./auth-task";

export interface AuthenticationResult {
    Task: EAuthTask,
    Token: string,
    ProfilePicUrl: string | null
}

