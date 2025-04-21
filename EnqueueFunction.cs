using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using MachogPatch.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace MachogPatch
{
    public class EnqueueFunction(QueueClient? queueClient,
                                 ServiceBusSender? sbSender,
                                 ILogger<EnqueueFunction> logger)
    {
        private readonly QueueClient?               _queueClient = queueClient;
        private readonly ServiceBusSender?          _sbSender = sbSender;
        private readonly ILogger<EnqueueFunction>   _logger = logger;

        [Function(nameof(EnqueueFunction))]
        [OpenApiOperation(operationId: nameof(Run), tags: ["Queue processing"],
                          Summary = "Enqueues the message for later asynchronous processing")]
        [OpenApiRequestBody(contentType: "application/json",
                            bodyType: typeof(ParkingProviderMessage),
                            Required = true,
                            Description = "The message to enqueue")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "messages")] HttpRequestData req,
                                             //[Microsoft.Azure.WebJobs.Queue("parking-provider", Connection = "AzureWebJobsStorage")] ICollector<string> queueCollector,
                                             CancellationToken ct = default)
        {
            try
            {
                using var reader = new StreamReader(req.Body);
                var requestBody = await reader.ReadToEndAsync(ct);

                // Try to deserialize just to ensure that the message is in correct format
                JsonSerializer.Deserialize<ParkingProviderMessage>(requestBody);

                //
                // The actual message will be written to the queue in base64 format
                string _message = Convert.ToBase64String(Encoding.UTF8.GetBytes(requestBody));
                Response response = await _queueClient.CreateIfNotExistsAsync(default, ct);

                if (!await _queueClient.ExistsAsync(ct))
                    throw new InvalidOperationException("Queue does not exist");

                //
                // Publish the message to Service Bus
                ServiceBusMessage message = new(_message);
                await _sbSender?.SendMessageAsync(message, ct);

                //
                // Pub;ish the message to Topic
                Response<SendReceipt> receipt = await _queueClient.SendMessageAsync(_message, ct);
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
