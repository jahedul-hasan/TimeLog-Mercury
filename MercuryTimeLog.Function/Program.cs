using MercuryTimeLog.Domain.Common;
using MercuryTimeLog.Function.Features;
using MercuryTimeLog.Function.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    .Build();

host.Run();
