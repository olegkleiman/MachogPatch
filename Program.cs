using Azure.Storage.Queues;
using MachogPatch.Services.ParkingProviderService;
using MachogPatch.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MachogPatch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices((context,services) =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();

                    services.AddHttpClient("machogParkingProviderClient", (provider, client) =>
                    {
                        var configuration = provider.GetRequiredService<IConfiguration>();
                        IConfigurationSection section = configuration.GetSection(nameof(ParkingProviderOptions));

                        ConfigUtils.ConfigureHttpClient(section, client);
                    });

                    services.AddSingleton<IParkingProviderService, ParkingProviderService>();

                    IConfiguration configuration = context.Configuration;

                    string queueConnectionString = configuration["AzureWebJobsStorage"];
                    string queueName = configuration["QueueName"];

                    services.AddSingleton( sp => new QueueClient(queueConnectionString, queueName));

                })
                .Build();

            host.Run();
        }
    }
}