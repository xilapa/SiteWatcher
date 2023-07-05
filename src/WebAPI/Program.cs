using FluentValidation;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Persistence;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Settings;
using DependencyInjection = SiteWatcher.Infra.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

#region AddServices

var appSettings = builder.Services.AddSettings(builder.Configuration, builder.Environment);

builder.Services.AddControllers()
    .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommand>();

builder.Services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDataContext();
builder.Services.AddDapperContext(DatabaseType.Postgres);
builder.Services.AddApplication();

builder.Services.AddRedisCache(appSettings)
    .SetupDataProtection(appSettings);

DependencyInjection.AddSession(builder.Services)
    .AddIdHasher()
    .SetupMassTransit(builder.Configuration);

var googleSettings = builder.Configuration.Get<GoogleSettings>();
builder.Services.ConfigureAuth(appSettings, googleSettings!);

builder.Services.AddHttpClient();

builder.Services.AddCors(options => {
    options.AddPolicy(name: appSettings.CorsPolicy,
        policyBuilder =>
        {
            policyBuilder.WithOrigins(appSettings.FrontEndUrl);
            policyBuilder.AllowAnyHeader();
            policyBuilder.WithMethods("OPTIONS", "GET", "POST", "PUT", "DELETE");
            policyBuilder.AllowCredentials();
        });
});

#endregion

#region HttpRequest Pipeline

var app = builder.Build();

var loggerFactory = app.Services.GetService<ILoggerFactory>();
app.ConfigureGlobalExceptionHandlerMiddleware(app.Environment, loggerFactory!);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "SiteWatcher.WebAPI");
        opt.RoutePrefix = "swagger";
    });
}

app.UseCors(appSettings.CorsPolicy);
app.UseHttpsRedirection();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto
});

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();

// To make Program visible to integration tests
// Official documentation recommendation
// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#basic-tests-with-the-default-webapplicationfactory
public partial class Program { }
