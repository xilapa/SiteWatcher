export enum EAlertFrequency
{
    TwoHours = 2,
    FourHours = 4,
    EightHours = 8,
    TwelveHours = 12,
    TwentyFourHours = 24
}

export class AlertFrequencyUtils{
    public static getFrequencyTranslationKey(frequency: EAlertFrequency): string{
        switch (frequency) {
            case EAlertFrequency.TwoHours:
                return "alert.frequency.twoHours";
            case EAlertFrequency.FourHours:
                return "alert.frequency.fourHours";
            case EAlertFrequency.EightHours:
                return "alert.frequency.eightHours";
            case EAlertFrequency.TwelveHours:
                return "alert.frequency.twelveHours";
            case EAlertFrequency.TwentyFourHours:
                return "alert.frequency.twentyFourHours";
        }
    }
}
