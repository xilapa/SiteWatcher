using Mediator;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.Events;

namespace SiteWatcher.Application.Users.EventHandlers;

public class UserUpdatedEventHandler : INotificationHandler<UserUpdatedEvent>
{
    private readonly ICache _cache;
    private readonly ISession _session;

    public UserUpdatedEventHandler(ICache cache, ISession session)
    {
        _cache = cache;
        _session = session;
    }

    public async ValueTask Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _cache.DeleteKeyAsync(CacheKeys.UserInfo(_session.UserId!.Value));
    }
}