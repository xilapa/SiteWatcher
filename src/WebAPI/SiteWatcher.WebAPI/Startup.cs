using IStartup = SiteWatcher.WebAPI.Interfaces.IStartup;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.Infra.Extensions;
using SiteWatcher.Infra.Data;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.Filters;

namespace SiteWatcher.WebAPI;

public class Startup : IStartup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration configuration) => Configuration = configuration; 

    // Add services to the container.
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {       
        services.AddSettings();

        services.AddControllers(opts => opts.Filters.Add(typeof(CommandValidationFilter)))
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
                
        services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDataContext<SiteWatcherContext>(env.IsDevelopment());
        services.AddRepositories();
        services.AddDomainServices();
        services.AddApplicationServices();
        services.AddApplicationFluentValidations();

        services.ConfigureAuth(Configuration);

        services.AddHttpClient();

        services.AddCors(options => {
            options.AddPolicy(name: "DevMode", // TODO: nome da política de cors deve ficar no appsettings
                              builder =>
                              {
                                  // TODO: essa origin deve ficar no appsettings
                                  builder.WithOrigins("http://localhost:4200");
                                  builder.AllowAnyHeader();
                                  builder.AllowAnyMethod();
                                //   builder.AllowCredentials();
                              });
        });
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

        app.UseCors("DevMode");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.ConfigureGlobalExceptionHandlerMiddleware(env, loggerFactory);
    }
}