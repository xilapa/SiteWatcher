export enum EWatchMode{
    AnyChanges = 1,
    Term,
    Regex
}

export class WatchModeUtils{
    public static getWatchModeTranslationKey(watchMode: EWatchMode): string{
        switch (watchMode) {
            case EWatchMode.AnyChanges:
                return "alert.watchMode.anyChanges"
            case EWatchMode.Term:
                return "alert.watchMode.term"
            case EWatchMode.Regex:
                return "alert.watchMode.regex"
        }
    }

    public static getNotifyOnDisappearanceTranslationKey(notifyOnDisappearance: (boolean | undefined)): string{
        if(notifyOnDisappearance === true)
            return "common.yes"

        return "common.no"
    }
}
