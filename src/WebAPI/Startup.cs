using SiteWatcher.Application;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.WebAPI.Extensions;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.Filters;
using SiteWatcher.WebAPI.Settings;
using Microsoft.AspNetCore.HttpOverrides;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;

namespace SiteWatcher.WebAPI;

public class Startup : IStartup
{
    public IAppSettings AppSettings { get; }

    public Startup(IConfiguration configuration) => AppSettings = configuration.Get<AppSettings>();

    // Add services to the container.
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddSettings();

        services.AddControllers(opts => {
                    opts.Filters.Add(typeof(CommandValidationFilter));
                    opts.Filters.Add(typeof(TokenValidationFilter));
                })
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

        services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDataContext<SiteWatcherContext>(env.IsDevelopment(), AppSettings.ConnectionString);
        services.AddRepositories();
        services.AddDapperRepositories(env.IsDevelopment(), AppSettings.ConnectionString);
        services.AddApplication();

        services.AddRedisCache(AppSettings.RedisConnectionString);

        services.ConfigureAuth(AppSettings);

        services.AddHttpClient();

        services.AddCors(options => {
            options.AddPolicy(name: AppSettings.CorsPolicy,
                              builder =>
                              {
                                  builder.WithOrigins(AppSettings.FrontEndUrl);
                                  builder.AllowAnyHeader();
                                  builder.WithMethods("OPTIONS", "GET", "POST", "PUT", "DELETE");
                              });
        });
    }

    // Configure the HTTP request pipeline.
    public void Configure(WebApplication app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
    {
        app.ConfigureGlobalExceptionHandlerMiddleware(env, loggerFactory);

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

        app.UseCors(AppSettings.CorsPolicy);

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto
        });

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();
    }
}