import { TranslocoService } from "@ngneat/transloco";
import { ConfirmationService, MessageService } from "primeng/api";

export class utils {
    private static successToastLifeTime = 5000;
    private static SHA256 = 'SHA-256';

    public static toastError(errorResponse: any, messageService: MessageService, translocoService: TranslocoService) {
        const messages = errorResponse.error as string[];
        let translatedErrors;
        if (!messages || !messages.length || messages.length === 0)
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

    public static toastSuccess(messageService: MessageService, translocoService: TranslocoService, translatedMessage: string) {
        messageService.add(
            {
                severity: 'success',
                summary: translocoService.translate('common.success'),
                detail: translatedMessage,
                sticky: false,
                closable: true,
                life: utils.successToastLifeTime
            }
        )
    }

    public static openModal(confirmationService: ConfirmationService, translocoService: TranslocoService,
                            headerKeyForTranslation: string, messageKeyForTranslation: string,
                            callback: Function | undefined = undefined,
                            rejectCallback: Function | undefined = undefined) {
        confirmationService.confirm({
            header: translocoService.translate(headerKeyForTranslation),
            message: translocoService.translate(messageKeyForTranslation),
            acceptLabel: translocoService.translate('common.yes'),
            rejectLabel: translocoService.translate('common.no'),
            defaultFocus: 'none',
            dismissableMask: true,
            accept: callback,
            reject: rejectCallback
        });
    }

    // Source: https://developer.mozilla.org/en-US/docs/Web/API/SubtleCrypto/digest
    public static async sha256Hash(value :string) : Promise<Uint8Array> {
        const utf8Value = new TextEncoder().encode(value);
        const hashBuffer = await crypto.subtle.digest(utils.SHA256, utf8Value);
        return new Uint8Array(hashBuffer);
      }
}
