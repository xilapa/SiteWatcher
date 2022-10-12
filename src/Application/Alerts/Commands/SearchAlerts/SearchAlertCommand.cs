using MediatR;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Extensions;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.SearchAlerts;

public class SearchAlertCommand : IRequest<CommandResult>, ICacheable
{
    public string Term { get; set; }
    public TimeSpan Expiration => TimeSpan.FromMinutes(2);
    public string HashFieldName => $"Term:{Term}";
    public string GetKey(ISession session)  =>
        CacheKeys.UserAlertSearch(session.UserId!.Value);
}

public class SearchAlertCommandHandler : IRequestHandler<SearchAlertCommand, CommandResult>
{
    private readonly IAlertDapperRepository _alertDapperRepository;
    private readonly ISession _session;
    private readonly IIdHasher _idHasher;

    public SearchAlertCommandHandler(IAlertDapperRepository alertDapperRepository, ISession session, IIdHasher idHasher)
    {
        _alertDapperRepository = alertDapperRepository;
        _session = session;
        _idHasher = idHasher;
    }

    public async Task<CommandResult> Handle(SearchAlertCommand request,
        CancellationToken cancellationToken)
    {
        var searchTerms = request
            .Term.Split(' ')
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => t.ToLowerCaseWithoutDiacritics()).ToArray();

        var alerts = await _alertDapperRepository
            .SearchSimpleAlerts(searchTerms, _session.UserId!.Value, 10, cancellationToken);
        if (alerts.Count == 0)
            return CommandResult.Empty();
        var alertsMapped = alerts.Select(dto => SimpleAlertView.FromDto(dto, _idHasher));
        return CommandResult.FromValue(alertsMapped);
    }
}