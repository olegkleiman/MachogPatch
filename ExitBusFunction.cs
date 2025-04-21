using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Messaging.ServiceBus;
using MachogPatch.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MachogPatch
{
    public class ExitBusFunction
    {
        private readonly ILogger<ExitBusFunction> _logger;

        public ExitBusFunction(ILogger<ExitBusFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ExitBusFunction))]
        public async Task Run(
            [ServiceBusTrigger("parking-provider", Connection = "SBConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            try
            {
                string base64Encoded = message.Body.ToString();
                byte[] decodedBytes = Convert.FromBase64String(base64Encoded);
                string decodedString = Encoding.UTF8.GetString(decodedBytes);

                var data = JsonSerializer.Deserialize<ParkingProviderMessage>(decodedString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deserializing message body: {ex.Message}");
                throw;
            }

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
