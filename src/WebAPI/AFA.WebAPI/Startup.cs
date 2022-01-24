using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IStartup = AFA.WebAPI.Interfaces.IStartup;
using AFA.WebAPI.Extensions;
using AFA.Infra.Extensions;
using AFA.Infra.Data;
using Microsoft.AspNetCore.Mvc;

namespace AFA.WebAPI;

public class Startup : IStartup
{
    public IConfiguration Configuration { get; }
    public Startup(IConfiguration configuration)
    {
        this.Configuration = configuration;
    }

    // Add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDataContext<AFAContext>();
        services.AddRepositories();
        services.AddDomainServices();
        services.AddApplicationServices();
        services.AddApplicationFluentValidations();
    }

    // Configure the HTTP request pipeline.
    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "AFA.WebAPI");
                opt.RoutePrefix = "swagger";
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
    }
}