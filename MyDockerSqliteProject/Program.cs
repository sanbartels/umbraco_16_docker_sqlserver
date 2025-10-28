using Microsoft.AspNetCore.HttpOverrides;

// ========================================
// Inicialización de base de datos
// ========================================
var baseDir = AppContext.BaseDirectory;
var dbDir = Path.Combine(baseDir, "umbraco", "Data");
var dbPath = Path.Combine(dbDir, "Umbraco.sqlite.db");
var seedDbPath = Path.Combine(baseDir, "seed-db", "Umbraco.sqlite.db");
var forceSeedDb = Environment.GetEnvironmentVariable("FORCE_SEED_DB") == "true";

Console.WriteLine("========================================");
Console.WriteLine("Inicializando base de datos de Umbraco...");
Console.WriteLine($"BaseDirectory: {baseDir}");
Console.WriteLine("========================================");

// Crear directorio si no existe
Directory.CreateDirectory(dbDir);

// Verificar si existe seed database
if (File.Exists(seedDbPath))
{
    Console.WriteLine($"✅ Seed DB encontrada: {seedDbPath}");
    var seedInfo = new FileInfo(seedDbPath);
    Console.WriteLine($"   Tamaño: {seedInfo.Length / 1024}KB");
}
else
{
    Console.WriteLine($"❌ No se encontró seed database en: {seedDbPath}");
}

// Decidir si copiar la seed database
if (forceSeedDb)
{
    Console.WriteLine("🔄 FORCE_SEED_DB=true - Sobrescribiendo base de datos...");
    if (File.Exists(seedDbPath))
    {
        File.Copy(seedDbPath, dbPath, overwrite: true);
        Console.WriteLine("✅ Base de datos seed copiada exitosamente (sobrescrita)");
        var dbInfo = new FileInfo(dbPath);
        Console.WriteLine($"   Tamaño: {dbInfo.Length / 1024}KB");
    }
    else
    {
        Console.WriteLine("❌ No se encontró seed database para copiar");
    }
}
else if (File.Exists(dbPath))
{
    Console.WriteLine($"✅ Base de datos existente encontrada: {dbPath}");
    var dbInfo = new FileInfo(dbPath);
    Console.WriteLine($"   Tamaño: {dbInfo.Length / 1024}KB");
    Console.WriteLine("   Usando base de datos actual (no se sobrescribe)");
    Console.WriteLine("   💡 Usa FORCE_SEED_DB=true para sobrescribir");
}
else
{
    Console.WriteLine($"⚠️  Base de datos no encontrada en: {dbPath}");
    if (File.Exists(seedDbPath))
    {
        Console.WriteLine("📦 Copiando base de datos seed...");
        File.Copy(seedDbPath, dbPath);
        Console.WriteLine("✅ Base de datos seed copiada exitosamente");
        var dbInfo = new FileInfo(dbPath);
        Console.WriteLine($"   Tamaño: {dbInfo.Length / 1024}KB");
    }
    else
    {
        Console.WriteLine("ℹ️  No hay seed database disponible");
        Console.WriteLine("   Umbraco creará una nueva base de datos");
    }
}

Console.WriteLine("========================================");

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

var backOfficeHost = builder.Configuration["Umbraco:CMS:Security:BackOfficeHost"]
                     ?? Environment.GetEnvironmentVariable("Umbraco__CMS__Security__BackOfficeHost")
                     ?? publicUrl;

// Log startup information
Console.WriteLine("========================================");
Console.WriteLine($"Umbraco Public URL: {publicUrl}");
Console.WriteLine($"BackOffice Host: {backOfficeHost}");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
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
