import {MessageService} from "primeng/api";
import {ApiResponse} from "../interfaces";
import {TranslocoService} from "@ngneat/transloco";

export class utils {
    public static successToastLifeTime = 5000;
    public static errorToast(errorResponse: any, messageService: MessageService, translocoService: TranslocoService) {
        const messages = (errorResponse.error as ApiResponse<null>)?.Messages;
        let translatedErrors;
        if(!messages || messages.length === 0)
            translatedErrors = translocoService.translate('common.unexpectedError');
        else
            translatedErrors = messages.map(m => translocoService.translate(m)).join('; ');

        messageService.add(
            {
                severity: 'error',
                summary: translocoService.translate('common.error'),
                detail: translatedErrors,
                sticky: true,
                closable: true
            }
        );
    }
}
