using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using SiteWatcher.WebAPI.Extensions;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.WebAPI.Filters;
using Microsoft.AspNetCore.HttpOverrides;
using SiteWatcher.Application;
using SiteWatcher.Application.Interfaces;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Authorization.Middleware;

namespace SiteWatcher.WebAPI;

public class Startup : IStartup
{
    private readonly IConfiguration _configuration;
    public IAppSettings AppSettings { get; set; }

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Add services to the container.
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {
        AppSettings = services.AddSettings(_configuration, env);

        services.AddControllers(opts => opts.Filters.Add(typeof(CommandValidationFilter)))
                .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null)
                .AddFluentValidation(opts =>
                {
                    opts.AutomaticValidationEnabled = false;
                    opts.RegisterValidatorsFromAssemblyContaining<RegisterUserCommand>();
                });

        services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDataContext<SiteWatcherContext>();
        services.AddRepositories();
        services.AddDapperRepositories();
        services.AddApplication();

        services.AddRedisCache(AppSettings);

        services.AddSessao()
            .AddEmailService()
            .AddFireAndForgetService();

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

        app.UseCors(AppSettings.CorsPolicy);
        app.UseHttpsRedirection();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto
        });

        app.UseAuthentication();
        // Session is instantiated first on authz handlers (AuthService) before the authz occurs
        // This middleware ensures that the session has the correct auth info on authz handlers
        // And through the request
        app.UseMiddleware<MultipleJwtsMiddleware>();
        app.UseAuthorization();

        app.MapControllers();
    }
}