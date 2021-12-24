using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AFA.WebAPI.Interfaces;

public interface IStartup
{
    IConfiguration Configuration { get; }
    void ConfigureServices(IServiceCollection services);
    void Configure(WebApplication app, IWebHostEnvironment env);
}
