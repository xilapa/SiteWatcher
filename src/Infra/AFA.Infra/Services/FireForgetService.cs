using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AFA.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AFA.Infra.Services;

public class FireForgetService : IFireForgetService
{
    private readonly IServiceScopeFactory serviceScopeFactory;

    public FireForgetService(IServiceScopeFactory  serviceScopeFactory)
    {
        this.serviceScopeFactory = serviceScopeFactory;
    }   

    [SuppressMessage("Suggestion", "IDE0063: 'using' statement can be simplified", Justification ="Using simplificado dificulta a leitura")]
    public void ExecuteWith<T>(Func<T, Task> func) where T: IInjectedService
    {
        Task.Run(async () => 
        {
            try
            {
                await Task.Delay(10000);
                using(var scope = serviceScopeFactory.CreateScope())
                {
                    var userRepo = scope.ServiceProvider.GetRequiredService<T>();
                    await func(userRepo);
                    Console.WriteLine("User added after request ended");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        });
    }
}