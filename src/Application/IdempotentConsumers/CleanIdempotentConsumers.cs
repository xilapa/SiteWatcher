﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Domain.Authentication;

namespace SiteWatcher.Application.IdempotentConsumers;

public sealed partial class CleanIdempotentConsumers
{
    private readonly ISiteWatcherContext _context;
    private readonly ILogger<CleanIdempotentConsumers> _logger;
    private readonly ISession _session;

    public CleanIdempotentConsumers(ISiteWatcherContext context, ILogger<CleanIdempotentConsumers> logger, ISession session)
    {
        _context = context;
        _logger = logger;
        _session = session;
    }

    public async Task Clean(CancellationToken cancellationToken)
    {
        var fiveDaysEarlier = _session.Now.Date.AddDays(-5);

        var rowsDeleted = await _context.IdempotentConsumers
            .Where(i => i.DateCreated < fiveDaysEarlier)
            .ExecuteDeleteAsync(cancellationToken);

        LogIdempotentConsumersCleaned(_session.Now, rowsDeleted);
    }

    [LoggerMessage(LogLevel.Information, "{Date} - IdempotentConsumers cleaned: {Rows} rows deleted")]
    public partial void LogIdempotentConsumersCleaned(DateTime date, int rows);
}