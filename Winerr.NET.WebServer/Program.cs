using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Winerr.NET.WebServer.Config;
using Winerr.NET.WebServer.Endpoints;
using Winerr.NET.WebServer.Middleware;

var random = new Random();
var goodbyePhrases = new[]
{
    "Shutdown sequence complete. Good Bye!",
    "See you next time!",
    "All systems down. Good Bye!",
    "Going offline...",
    "Execution finished. Have a nice day!",
    "Don't forget to star the repo on GitHub!"
};

var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
var archivedLogPath = $"logs/log-{timestamp}.txt";
var latestLogPath = "logs/latest.log";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        theme: ConsoleTheme.None)
    .WriteTo.File(archivedLogPath)
    .WriteTo.File(latestLogPath)
    .CreateBootstrapLogger();

var logger = Log.ForContext<Program>();

logger.Information("============================================================");
logger.Information("             STARTING NEW Winerr.NET SERVER RUN             ");
logger.Information("============================================================");

ServerConfig config;
try
{
    config = ConfigLoader.LoadConfig(logger);
    logger.Information("Configuration loaded successfully.");
}
catch (Exception ex)
{
    logger.Fatal(ex, "Failed to load server configuration. Shutting down.");
    Environment.Exit(1);
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls(config.Server.Url);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
        theme: ConsoleTheme.None)
    .WriteTo.File(archivedLogPath)
    .WriteTo.File(latestLogPath));

builder.Services.AddSingleton(config);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });
builder.Services.AddMemoryCache();

Winerr.NET.Core.Managers.AssetManager.Instance.LoadAssets();
builder.Services.AddSingleton(Winerr.NET.Core.Managers.AssetManager.Instance);

logger.Information("Assets loaded successfully.");

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
appLifetime.ApplicationStopping.Register(() =>
{
    logger.Information("Server is shutting down...");
    Winerr.NET.Core.Managers.AssetManager.Instance.Dispose();
    logger.Information("Assets disposed successfully.");
    var goodbyeMessage = goodbyePhrases[random.Next(goodbyePhrases.Length)];
    logger.Information(goodbyeMessage);
    logger.Information("============================================================");
    Log.CloseAndFlush();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();

app.Use((context, next) =>
{
    context.Request.EnableBuffering();
    return next();
});
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<AuthenticationMiddleware>();

app.MapStyleEndpoints();
app.MapImageEndpoints();
app.MapIconEndpoints();
app.MapHealthEndpoints();
app.MapAssetEndpoints();
app.MapFontEndpoints();

app.Run();
