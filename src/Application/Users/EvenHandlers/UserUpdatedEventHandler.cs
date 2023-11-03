using SiteWatcher.Application.Common.Command;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Services;
using SiteWatcher.Domain.Users.Events;

namespace SiteWatcher.Application.Users.EventHandlers;

public class UserUpdatedEventHandler : IApplicationHandler
{
    private readonly ICache _cache;

    public UserUpdatedEventHandler(ICache cache)
    {
        _cache = cache;
    }

    public async Task Handle(UserUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _cache.DeleteKeyAsync(CacheKeys.UserInfo(notification.UserId));
    }
}