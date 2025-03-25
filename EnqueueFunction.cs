using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using MachogPatch.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MachogPatch
{
    public class EnqueueFunction(IConfiguration? configuration,
                                 ILogger<EnqueueFunction> logger)
    {
        private readonly IConfiguration?            _configuration = configuration;
        private readonly ILogger<EnqueueFunction>   _logger = logger;

        [Function(nameof(EnqueueFunction))]
        [OpenApiOperation(operationId: nameof(Run), tags: ["Queue processing"],
                          Summary = "Enqueues the message for later asynchronous processing")]
        [OpenApiRequestBody(contentType: "application/json",
                            bodyType: typeof(ParkingProviderMessage),
                            Required = true,
                            Description = "The message to enqueue")]

        public async Task<IActionResult> Run([Microsoft.Azure.Functions.Worker.HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
                                             //[Microsoft.Azure.WebJobs.Queue("parking-provider", Connection = "AzureWebJobsStorage")] ICollector<string> queueCollector,
                                             CancellationToken ct = default)
        {
            try
            {
                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync(ct);

                var message = JsonSerializer.Deserialize<ParkingProviderMessage>(requestBody);

                string queueConnectionString = _configuration["AzureWebJobsStorage"];
                string queueName = _configuration["QueueName"];
                QueueClient queueClient = new(queueConnectionString, queueName);
                Response<SendReceipt> receipt = await queueClient.SendMessageAsync(requestBody, ct);

                return new OkObjectResult(receipt.Value.MessageId);
            }
            catch (Exception ex) when (ex is TaskCanceledException)
            {
                _logger.LogError($"==> {ex.Message}");
                return new OkObjectResult("");
            }
            catch (Exception ex)
            {
                _logger.LogError($"==> {ex.Message}");

                return new ObjectResult(ex.Message)
                {
                    StatusCode = 500
                };
            }
        }
    }
}
