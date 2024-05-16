using MercuryTimeLog.Domain.Common;
using MercuryTimeLog.Function.Features;
using MercuryTimeLog.Function.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context,services) =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        //services.AddScoped<IJsonService,JsonService>();
        services.AddScoped<ITableStorageHelperService, TableStorageHelperService>();
        services.AddScoped<ITimeLogApiService,TimeLogApiService>();

    })
    .UseSerilog((_, conf) =>
    {
        conf
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    })
    .Build();


host.Run();
