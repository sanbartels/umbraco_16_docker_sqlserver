using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Server;

namespace MyDockerSqliteProject;

public class OpenIddictComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Configure OpenIddict to allow HTTP connections (needed when behind HTTP proxy)
        builder.Services.Configure<OpenIddictServerOptions>(options =>
        {
            // Allow HTTP connections (disable HTTPS requirement for development/testing)
            options.DisableTransportSecurityRequirement = true;

            // Log for debugging
            Console.WriteLine("OpenIddict configured to allow HTTP connections");
        });
    }
}
