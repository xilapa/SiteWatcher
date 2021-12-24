using Microsoft.AspNetCore.Builder;
using AFA.WebAPI.Extensions;
using AFA.WebAPI;

WebApplication
    .CreateBuilder(args)
    .UseStartup<Startup>();