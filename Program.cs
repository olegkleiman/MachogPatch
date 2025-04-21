using Azure.Messaging.ServiceBus;
using Azure.Storage.Queues;
using MachogPatch.Services.ParkingProviderService;
using MachogPatch.Utils;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
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
                .ConfigureServices(async (context,services) =>
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

                    string? queueConnectionString = configuration["AzureWebJobsStorage"];
                    string? queueName = configuration["QueueName"];
                    services.AddSingleton<IConfiguration>(context.Configuration);

                    services.AddSingleton(sp => new QueueClient(queueConnectionString, queueName));

                    //
                    // Service Bus staff
                    //
                    string? sbConnectionString = configuration["SBConnectionString"];
                    services.AddAzureClients(clientBuilder =>
                    {
                        clientBuilder.AddServiceBusClient(sbConnectionString)
                                        .WithName("MyServiceBusClient");
                    });
                    services.AddSingleton<ServiceBusClient>(provider =>
                    {
                        var clientFactory = provider.GetRequiredService<IAzureClientFactory<ServiceBusClient>>();
                        ServiceBusClient client = clientFactory.CreateClient("MyServiceBusClient");

                        return client;
                    });
                    //services.AddSingleton<ServiceBusSender>(provider => {
                    //    var clientFactory = provider.GetRequiredService<IAzureClientFactory<ServiceBusClient>>();
                    //    var client = clientFactory.CreateClient("MyServiceBusClient");
                    //    string? sbQueueName = configuration["SBQueueName"];
                    //    return client.CreateSender(sbQueueName);

                    //});
                    //services.AddSingleton<ServiceBusSender>(provider =>
                    //{
                    //    var clientFactory = provider.GetRequiredService<IAzureClientFactory<ServiceBusClient>>();
                    //    var client = clientFactory.CreateClient("MyServiceBusClient");
                    //    string? sbTopicName = configuration["SQTopicName"];
                    //    return client.CreateSender(sbTopicName);

                    //});
                })
                .Build();

            host.Run();
        }
    }
}