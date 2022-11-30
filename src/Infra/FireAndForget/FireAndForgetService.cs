using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SiteWatcher.Application.Interfaces;

namespace SiteWatcher.Infra.FireAndForget;

public class FireAndForgetService : IFireAndForgetService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILoggerFactory _loggerFactory;

    public FireAndForgetService(IServiceScopeFactory serviceScopeFactory, ILoggerFactory loggerFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _loggerFactory = loggerFactory;
    }

    public void ExecuteWith<T>(Func<T, Task> func) where T: notnull
    {
        Task.Run(async () =>
        {
            var logger = _loggerFactory.CreateLogger($"{nameof(FireAndForgetService)}-{typeof(T).Name}");
            try
            {
                logger.LogInformation("FireAndForgetService started");
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var service = scope.ServiceProvider.GetRequiredService<T>();
                if (service is null)
                {
                    logger.LogError("The service is null, FireAndForgetService stopped");
                    return;
                }

                await func(service);
                logger.LogInformation("FireAndForgetService finalized");
            }
            catch (Exception e)
            {
                logger.LogError("FireAndForgetService stopped with exception: {Exception}", e);
            }
        });
    }

    public void ExecuteWith<T1,T2>(Func<T1, T2, Task> func)
        where T1: notnull
        where T2: notnull
    {
        Task.Run(async () =>
        {
            var logger = _loggerFactory.CreateLogger($"{nameof(FireAndForgetService)}-{typeof(T1).Name} and {typeof(T2).Name}");
            try
            {
                logger.LogInformation("FireAndForgetService started");
                await using var scope = _serviceScopeFactory.CreateAsyncScope();
                var service1 = scope.ServiceProvider.GetRequiredService<T1>();
                var service2 = scope.ServiceProvider.GetRequiredService<T2>();
                if (service1 is null)
                {
                    logger.LogError("The service {Service1} is null, FireAndForgetService stopped", typeof(T1).Name);
                    return;
                }

                if (service2 is null)
                {
                    logger.LogError("The service {Service1} is null, FireAndForgetService stopped", typeof(T2).Name);
                    return;
                }
                await func(service1, service2);
                logger.LogInformation("FireAndForgetService finalized");
            }
            catch (Exception e)
            {
                logger.LogError("FireAndForgetService stopped with exception: {Exception}", e);
            }
        });
    }
}