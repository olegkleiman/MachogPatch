using MachogPatch.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;

namespace MachogPatch
{
    public class EnqueueFunction(ILogger<EnqueueFunction> logger)
    {
        private readonly ILogger<EnqueueFunction> _logger = logger;

        [Function(nameof(EnqueueFunction))]
        [OpenApiOperation(operationId: nameof(Run), tags: ["Queue processing"],
                          Summary = "Enqueues the message for later asynchronous processing")]
        [OpenApiRequestBody(contentType: "application/json", 
                            bodyType: typeof(ParkingProviderMessage),
                            Required = true, 
                            Description = "The message to enqueue")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
                                             CancellationToken ct = default)
        {
            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync(ct);

                return new OkObjectResult("Welcome to Azure Functions!");
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
