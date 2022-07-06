export enum EWatchMode{
    AnyChanges = 1,
    Term
}

export class WatchModeUtils{
    public static getWatchModeTranslationKey(watchMode: EWatchMode): string{
        switch (watchMode) {
            case EWatchMode.AnyChanges:
                return "alert.watchMode.anyChanges";
            case EWatchMode.Term:
                return "alert.watchMode.term";
        }
    }
}
