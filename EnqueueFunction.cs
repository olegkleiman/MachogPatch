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
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace MachogPatch
{
    public class EnqueueFunction(QueueClient? queueClient,
                                 ServiceBusClient sbClient,
                                 IConfiguration configuration,
                                 ILogger<EnqueueFunction> logger)
    {
        private readonly QueueClient?               _queueClient = queueClient;
        private readonly ServiceBusClient           _sbclient = sbClient;
        private readonly IConfiguration             _configuration = configuration;

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
                ParkingProviderMessage? _message = JsonSerializer.Deserialize<ParkingProviderMessage>(requestBody) 
                        ?? throw new ArgumentNullException("Could not desrialize the payload");

                //
                // The actual message will be written to the queue in base64 format
                string _messageToSend = Convert.ToBase64String(Encoding.UTF8.GetBytes(requestBody));
                Response response = await _queueClient.CreateIfNotExistsAsync(default, ct);

                if (!await _queueClient.ExistsAsync(ct))
                    throw new InvalidOperationException("Queue does not exist");

                //
                // Publish the message to Storage Account Queue
                Response<SendReceipt> receipt = await _queueClient.SendMessageAsync(_messageToSend, ct);

                //
                // Publish the message to Service Bus Queue
                string sbQueueName = _configuration["SBQueueName"];
                ServiceBusSender queueSender = _sbclient.CreateSender(sbQueueName);
                ServiceBusMessage message = new(_messageToSend)
                {
                    ApplicationProperties =
                    {
                        { "app_id", _message.AppID }
                    }
                };
                await queueSender?.SendMessageAsync(message, ct);

                // 
                // Publish to Service Bus Topic
                string sbTopicName = _configuration["SQTopicName"];
                ServiceBusSender topicSender = _sbclient.CreateSender(sbTopicName);

                await topicSender?.SendMessageAsync(message, ct);

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
