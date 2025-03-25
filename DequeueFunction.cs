using Azure.Storage.Queues.Models;
using MachogPatch.Entities;
using MachogPatch.Services.ParkingProviderService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace MachogPatch
{
    public class DequeueFunction(IParkingProviderService parkingService,
                                ILogger<DequeueFunction> logger)
    {
        private readonly IParkingProviderService _parkingService = parkingService;
        private readonly ILogger<DequeueFunction> _logger = logger;

        [Function(nameof(DequeueFunction))]
        public async Task<IActionResult> Run([QueueTrigger("parking-provider", Connection = "AzureWebJobsStorage")] QueueMessage message,
                                             CancellationToken ct = default)
        {
            _logger.LogInformation($"C# Queue trigger function processed: {message.MessageText}");

            try
            {
                ParkingProviderMessage? providerMessage = JsonSerializer.Deserialize<ParkingProviderMessage>(message.MessageText);
                if (providerMessage is null)
                {
                    throw new ArgumentNullException("providerMessage");
                }

                await _parkingService.UpdateRegistration(providerMessage, ct);
                return new OkObjectResult(true);
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
