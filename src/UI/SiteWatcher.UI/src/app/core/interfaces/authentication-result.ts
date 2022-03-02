import { EAuthTask } from '../enums/';

export interface AuthenticationResult {
    Task: EAuthTask,
    Token: string
}

