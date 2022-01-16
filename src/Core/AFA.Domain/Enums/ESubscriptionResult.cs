namespace AFA.Domain.Enums;

public enum ESubscriptionResult
{
    SubscribedSuccessfully = 1,
    AlreadySubscribed = 2,
    Unsubscribed = 3,
    EmailNotConfirmed = 4,
    UserDoNotExist = 5
}