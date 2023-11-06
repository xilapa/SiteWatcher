using Application.Alerts.Dtos;
using Dapper;
using Mediator;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Extensions;

namespace SiteWatcher.Application.Alerts.Commands.SearchAlerts;

public class SearchAlertQuery : IQuery<IEnumerable<SimpleAlertView>>, ICacheable
{
    public string Term { get; set; } = null!;
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    public string HashFieldName => $"Term:{Term}";
    public string GetKey(ISession session) =>
        CacheKeys.UserAlertSearch(session.UserId!.Value);
}

public class SearchAlertQueryHandler : IQueryHandler<SearchAlertQuery, IEnumerable<SimpleAlertView>>
{
    private readonly IDapperContext _context;
    private readonly IQueries _queries;
    private readonly ISession _session;
    private readonly IIdHasher _idHasher;

    public SearchAlertQueryHandler(IDapperContext context, IQueries queries, ISession session, IIdHasher idHasher)
    {
        _context = context;
        _queries = queries;
        _session = session;
        _idHasher = idHasher;
    }

    public async ValueTask<IEnumerable<SimpleAlertView>> Handle(SearchAlertQuery request, CancellationToken cancellationToken)
    {
        var searchTerms = request
            .Term.Split(' ')
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => t.ToLowerCaseWithoutDiacritics()).ToArray();

        var query = _queries.SearchSimpleAlerts(_session.UserId!.Value, searchTerms, 10);

        var simpleAlertViewDtos = await _context
            .UsingConnectionAsync(conn =>
                {
                    var cmd = new CommandDefinition(
                        query.Sql,
                        query.Parameters,
                        cancellationToken: cancellationToken);
                    return conn.QueryAsync<SimpleAlertViewDto>(cmd);
                });
        return simpleAlertViewDtos.Select(dto => dto.ToSimpleAlertView(_idHasher));
    }
}