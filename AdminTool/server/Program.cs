using Microsoft.Extensions.Logging;
using Server.Application.Abstractions;
using Server.Application.DependencyInjection;
using Server.Infrastructure.DependencyInjection;
using Server.Presentation.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions =
        ActivityTrackingOptions.SpanId |
        ActivityTrackingOptions.TraceId |
        ActivityTrackingOptions.ParentId |
        ActivityTrackingOptions.Tags |
        ActivityTrackingOptions.Baggage;
});
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.UseUtcTimestamp = true;
    options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
});

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

var startupLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
startupLogger.LogInformation("API is now starting.");

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IClaimsApplicationService>().Initialize();
    scope.ServiceProvider.GetRequiredService<IHealthInsuranceApplicationService>().Initialize();
}

startupLogger.LogInformation("Domain seeds initialized.");
app.Lifetime.ApplicationStarted.Register(() => startupLogger.LogInformation("API has started."));

app.InitializeData();
app.UsePresentation();

app.Run();
