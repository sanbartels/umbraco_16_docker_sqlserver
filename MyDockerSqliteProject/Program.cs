using Microsoft.AspNetCore.HttpOverrides;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for reverse proxy (Coolify)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = null; // Allow any number of proxies
    options.RequireHeaderSymmetry = false;
});

// Get the public URL from environment or configuration
var publicUrl = builder.Configuration["Umbraco:CMS:WebRouting:UmbracoApplicationUrl"]
                ?? Environment.GetEnvironmentVariable("Umbraco__CMS__WebRouting__UmbracoApplicationUrl")
                ?? "http://localhost:8080";

// Log startup information
Console.WriteLine("========================================");
Console.WriteLine($"Umbraco Public URL: {publicUrl}");
Console.WriteLine("========================================");

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

// Enable forwarded headers BEFORE other middleware (required for reverse proxy like Coolify)
app.UseForwardedHeaders();

await app.BootUmbracoAsync();


app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
