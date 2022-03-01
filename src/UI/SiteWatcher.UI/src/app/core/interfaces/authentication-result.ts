import { EAuthTask } from '../enums/';
import { UserRegister } from './user-register';

export interface AuthenticationResult {
    Task: EAuthTask,
    RegisterModel: UserRegister,
    Token: string
}

