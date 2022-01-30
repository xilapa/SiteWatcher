using System;
using System.Threading;
using System.Threading.Tasks;
using AFA.Domain.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AFA.Domain.Entities;

namespace AFA.WebAPI.HostedServices;

public class TimedService : IHostedService
{
    private readonly ILogger<TimedService> logger;
    private readonly IBackgroundTaskQueue taskQueue;

    public TimedService(ILogger<TimedService> logger, IBackgroundTaskQueue taskQueue)
    {
        this.logger = logger;
        this.taskQueue = taskQueue;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting de hosted service");
        var exectuingTask = ExecuteAsync(cancellationToken);

        if(exectuingTask.IsCompleted)
            return exectuingTask;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }


    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {   
        while (!cancellationToken.IsCancellationRequested)
        {                
            logger.LogInformation("Background Task enqueued started at {time}", DateTime.Now);

            Func<CancellationToken, dynamic, Task> funcao = async (token, objeto) =>
                        {
                            var newId = Guid.NewGuid();
                            var userNovo = new User("nome", "email") { Id = newId };
                            objeto.Add(userNovo);

                            await (objeto.UoW.SaveChangesAsync() as Task<int>);
                            logger.LogInformation("Background Task Id: {id} executed at {time}", newId, DateTime.Now);
                        };

            await taskQueue.QueueBackgroundWorkItemAsync(
                (
                    func: funcao, 
                    tipo: typeof(IUserRepository)
                )
            );

            logger.LogInformation("Background Task enqueued finished at {time}", DateTime.Now);
            await Task.Delay(10000, cancellationToken);

        }        
    }
}