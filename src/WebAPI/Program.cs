using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using SiteWatcher.Application;
using SiteWatcher.Application.Users.Commands.RegisterUser;
using SiteWatcher.Infra;
using SiteWatcher.Infra.Authorization;
using SiteWatcher.Infra.Authorization.Middleware;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI.Filters;

var builder = WebApplication.CreateBuilder(args);

#region AddServices

var appSettings = builder.Services.AddSettings(builder.Configuration, builder.Environment);

builder.Services.AddControllers(opts => opts.Filters.Add(typeof(CommandValidationFilter)))
    .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null)
    .AddFluentValidation(opts =>
    {
        opts.AutomaticValidationEnabled = false;
        opts.RegisterValidatorsFromAssemblyContaining<RegisterUserCommand>();
    });

builder.Services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDataContext<SiteWatcherContext>();
builder.Services.AddRepositories();
builder.Services.AddDapperRepositories();
builder.Services.AddApplication();

builder.Services.AddRedisCache(appSettings);

builder.Services.AddSessao()
    .AddEmailService()
    .AddFireAndForgetService();

builder.Services.ConfigureAuth(appSettings);

builder.Services.AddHttpHandler();

builder.Services.AddCors(options => {
    options.AddPolicy(name: appSettings.CorsPolicy,
        policyBuilder =>
        {
            policyBuilder.WithOrigins(appSettings.FrontEndUrl);
            policyBuilder.AllowAnyHeader();
            policyBuilder.WithMethods("OPTIONS", "GET", "POST", "PUT", "DELETE");
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
// Session is instantiated first on authz handlers (AuthService) before the authz occurs
// This middleware ensures that the session has the correct auth info on authz handlers
// And through the request
app.UseMiddleware<MultipleJwtsMiddleware>();
app.UseAuthorization();

app.MapControllers();

#endregion

app.Run();

// To make Program visible to integration tests
// Official documentation recomendation
// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-6.0#basic-tests-with-the-default-webapplicationfactory
public partial class Program { }
