using System.Dynamic;
using Application.Alerts.Dtos;
using Dapper;
using MediatR;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.Extensions;

namespace SiteWatcher.Application.Alerts.Commands.SearchAlerts;

public class SearchAlertCommand : IRequest<IEnumerable<SimpleAlertView>>, ICacheable
{
    public string Term { get; set; } = null!;
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    public string HashFieldName => $"Term:{Term}";
    public string GetKey(ISession session) =>
        CacheKeys.UserAlertSearch(session.UserId!.Value);
}

public class SearchAlertCommandHandler : IRequestHandler<SearchAlertCommand, IEnumerable<SimpleAlertView>>
{
    private readonly IDapperContext _context;
    private readonly IQueries _queries;
    private readonly ISession _session;
    private readonly IIdHasher _idHasher;
    private const string userIdParam = "userId";
    private const string takeParam = "take";

    public SearchAlertCommandHandler(IDapperContext context, IQueries queries, ISession session, IIdHasher idHasher)
    {
        _context = context;
        _queries = queries;
        _session = session;
        _idHasher = idHasher;
    }

    public async Task<IEnumerable<SimpleAlertView>> Handle(SearchAlertCommand request, CancellationToken cancellationToken)
    {
        var searchTerms = request
            .Term.Split(' ')
            .Where(t => !string.IsNullOrEmpty(t))
            .Select(t => t.ToLowerCaseWithoutDiacritics()).ToArray();

        var parameters = new ExpandoObject() as IDictionary<string, object>;
        parameters[userIdParam] = _session.UserId!;
        parameters[takeParam] = 10;

        for (var i = 0; i < searchTerms.Length; i++)
        {
            parameters.Add($"searchTermWildCards{i}", $"%{searchTerms[i]}%");
            parameters.Add($"searchTerm{i}", searchTerms[i]);
        }

        var simpleAlertViewDtos = await _context
            .UsingConnectionAsync(conn =>
                {
                    var cmd = new CommandDefinition(
                        _queries.SearchSimpleAlerts(searchTerms.Length),
                        parameters,
                        cancellationToken: cancellationToken);
                    return conn.QueryAsync<SimpleAlertViewDto>(cmd);
                });
        return simpleAlertViewDtos.Select(dto => dto.ToSimpleAlertView(_idHasher));
    }
}