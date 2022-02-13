using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SiteWatcher.WebAPI.Interfaces;

public interface IStartup
{
    IConfiguration Configuration { get; }
    void ConfigureServices(IServiceCollection services, IWebHostEnvironment env);
    void Configure(WebApplication app, IWebHostEnvironment env, ILoggerFactory loggerFactory);
}
