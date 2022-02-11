using Microsoft.AspNetCore.Builder;
using SiteWatcher.WebAPI.Extensions;
using SiteWatcher.WebAPI;

WebApplication
    .CreateBuilder(args)
    .UseStartup<Startup>();