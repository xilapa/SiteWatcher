export enum Rules{
    AnyChanges = 65,
    Term = 84,
    Regex = 82
}

export class RuleUtils{
    public static getRuleTranslationKey(rule: Rules): string{
        switch (rule) {
            case Rules.AnyChanges:
                return "alert.rule.anyChanges"
            case Rules.Term:
                return "alert.rule.term"
            case Rules.Regex:
                return "alert.rule.regex"
        }
    }

    public static getNotifyOnDisappearanceTranslationKey(notifyOnDisappearance: (boolean | undefined)): string{
        if(notifyOnDisappearance === true)
            return "common.yes"

        return "common.no"
    }
}
