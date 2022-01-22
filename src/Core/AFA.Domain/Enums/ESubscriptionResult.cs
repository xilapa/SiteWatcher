using System.ComponentModel;

namespace AFA.Domain.Enums;

public enum ESubscriptionResult
{
    [Description("User subscribed successfully")]
    SubscribedSuccessfully = 1,

    [Description("User already subscribed")]
    AlreadySubscribed = 2,

    [Description("User unsubscribed successfully")]
    Unsubscribed = 3,

    [Description("Email not confirmed")]
    EmailNotConfirmed = 4,

    [Description("User does not exist")]
    UserDoesNotExist = 5
}