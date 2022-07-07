export enum AlertFrequency
{
    TwoHours = 2,
    FourHours = 4,
    EightHours = 8,
    TwelveHours = 12,
    TwentyFourHours = 24
}

export class AlertFrequencyUtils{
    public static getFrequencyTranslationKey(frequency: AlertFrequency): string{
        switch (frequency) {
            case AlertFrequency.TwoHours:
                return "alert.frequency.twoHours";
            case AlertFrequency.FourHours:
                return "alert.frequency.fourHours";
            case AlertFrequency.EightHours:
                return "alert.frequency.eightHours";
            case AlertFrequency.TwelveHours:
                return "alert.frequency.twelveHours";
            case AlertFrequency.TwentyFourHours:
                return "alert.frequency.twentyFourHours";
        }
    }
}
