using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
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

            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
