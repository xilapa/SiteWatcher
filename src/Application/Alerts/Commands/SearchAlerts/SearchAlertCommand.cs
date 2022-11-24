using MediatR;
using SiteWatcher.Application.Alerts.ViewModels;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.SearchAlerts;

public class SearchAlertCommand : IRequest<IEnumerable<SimpleAlertView>>, ICacheable
{
    public string Term { get; set; } = null!;
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    public string HashFieldName => $"Term:{Term}";
    public string GetKey(ISession session)  =>
        CacheKeys.UserAlertSearch(session.UserId!.Value);
}

public class SearchAlertCommandHandler : IRequestHandler<SearchAlertCommand, IEnumerable<SimpleAlertView>>
{
    private readonly IAlertDapperRepository _alertDapperRepository;
    private readonly ISession _session;

    public SearchAlertCommandHandler(IAlertDapperRepository alertDapperRepository, ISession session)
    {
        _alertDapperRepository = alertDapperRepository;
        _session = session;
    }

    public async Task<IEnumerable<SimpleAlertView>> Handle(SearchAlertCommand request, CancellationToken cancellationToken)
    {
        var searchTerms = request
            .Term.Split(' ')
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => t.ToLowerCaseWithoutDiacritics()).ToArray();

        var simpleAlerts = await _alertDapperRepository
            .SearchSimpleAlerts(searchTerms, _session.UserId!.Value, 10, cancellationToken);
        return simpleAlerts;
    }
}