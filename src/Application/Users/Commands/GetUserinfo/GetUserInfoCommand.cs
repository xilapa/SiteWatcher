using Dapper;
using MediatR;
using SiteWatcher.Application.Common.Queries;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common.Constants;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Domain.Users.DTOs;

namespace SiteWatcher.Application.Users.Commands.GetUserinfo;

public sealed class GetUserInfoCommand : IRequest<UserViewModel?>, ICacheable
{
    public TimeSpan Expiration => TimeSpan.FromMinutes(15);
    public string HashFieldName => string.Empty;

    public string GetKey(ISession session) =>
        CacheKeys.UserInfo(session.UserId!.Value);
}

public sealed class GetUserInfoCommandHandler : IRequestHandler<GetUserInfoCommand, UserViewModel?>
{
    private readonly IDapperContext _context;
    private readonly IQueries _queries;
    private readonly ISession _session;

    public GetUserInfoCommandHandler(IDapperContext context, IQueries queries, ISession session)
    {
        _context = context;
        _queries = queries;
        _session = session;
    }

    public Task<UserViewModel?> Handle(GetUserInfoCommand request, CancellationToken ct)
    {
        if (UserId.Empty.Equals(_session.UserId))
            return Task.FromResult<UserViewModel?>(null);

        var query = _queries.GetUserById(_session.UserId!.Value);
        return _context.UsingConnectionAsync(conn =>
        {
            var cmd = new CommandDefinition(
                query.Sql,
                query.Parameters,
                cancellationToken: ct);

            return conn.QuerySingleOrDefaultAsync<UserViewModel?>(cmd);
        });
    }
}