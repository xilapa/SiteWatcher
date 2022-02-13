using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IStartup = SiteWatcher.WebAPI.Interfaces.IStartup;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.Infra.Extensions;
using SiteWatcher.Infra.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SiteWatcher.WebAPI;

public class Startup : IStartup
{
    public IConfiguration Configuration { get; }
    private readonly Settings _settings;
    public Startup(IConfiguration configuration)
    {
        this.Configuration = configuration;
        _settings = Configuration.Get<Settings>();
    }

    // Add services to the container.
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {        
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDataContext<SiteWatcherContext>(env.IsDevelopment());
        services.AddRepositories();
        services.AddDomainServices();
        services.AddApplicationServices();
        services.AddApplicationFluentValidations();
    }

    // Configure the HTTP request pipeline.
    public void Configure(WebApplication app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "SiteWatcher.WebAPI");
                opt.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.ConfigureGlobalExceptionHandlerMiddleware(env, loggerFactory);
    }
}