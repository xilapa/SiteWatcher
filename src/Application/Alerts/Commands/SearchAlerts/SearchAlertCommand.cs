using AutoMapper;
using MediatR;
using SiteWatcher.Application.Alerts.Commands.GetUserAlerts;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Extensions;
using SiteWatcher.Domain.Utils;

namespace SiteWatcher.Application.Alerts.Commands.SearchAlerts;

public class SearchAlertCommand : IRequest<ICommandResult<IEnumerable<SimpleAlertView>>>, ICacheable
{
    public string Term { get; set; }
    public TimeSpan Expiration => TimeSpan.FromMinutes(2);
    public string HashFieldName => $"Term:{Term}";
    public string GetKey(ISession session)  =>
        CacheKeys.UserAlertSearch(session.UserId!.Value);
}

public class
    SearchAlertCommandHandler : IRequestHandler<SearchAlertCommand, ICommandResult<IEnumerable<SimpleAlertView>>>
{
    private readonly IAlertDapperRepository _alertDapperRepository;
    private readonly ISession _session;
    private readonly IMapper _mapper;

    public SearchAlertCommandHandler(IAlertDapperRepository alertDapperRepository, ISession session, IMapper mapper)
    {
        _alertDapperRepository = alertDapperRepository;
        _session = session;
        _mapper = mapper;
    }

    public async Task<ICommandResult<IEnumerable<SimpleAlertView>>> Handle(SearchAlertCommand request,
        CancellationToken cancellationToken)
    {
        var searchTerm = request.Term.ToLowerCaseWithoutDiacritics();
        var alerts = await _alertDapperRepository
            .SearchSimpleAlerts(searchTerm, _session.UserId!.Value, 10, cancellationToken);
        if (alerts.Count == 0)
            return new CommandResult<IEnumerable<SimpleAlertView>>(Array.Empty<SimpleAlertView>());
        var alertsMapped = _mapper.Map<IEnumerable<SimpleAlertView>>(alerts);
        return new CommandResult<IEnumerable<SimpleAlertView>>(alertsMapped);
    }
}