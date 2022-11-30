using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Infra.Extensions;

public static class MediatorExtensions
{
    public static async Task DispatchDomainEvents(this IMediator mediator, DbContext context)
    {
        var models = context
            .ChangeTracker
            .Entries<IBaseModel>()
            .Where(m => m.Entity.DomainEvents.Length != 0)
            .Select(m => m.Entity)
            .ToArray();

        if(models.Length == 0)
            return;

        foreach (var model in models)
        {
            foreach (var domainEvent in model.DomainEvents)
                await mediator.Publish(domainEvent);

            model.ClearDomainEvents();
        }
    }
}