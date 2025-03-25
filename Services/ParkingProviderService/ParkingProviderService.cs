using MachogPatch.Entities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http.Json;

namespace MachogPatch.Services.ParkingProviderService
{
    public class ParkingProviderService(IHttpClientFactory factory,
                                        ILogger<ParkingProviderService> logger) : IParkingProviderService
    {
        private readonly HttpClient _httpClient = factory.CreateClient("machogParkingProviderClient");
        private readonly ILogger<ParkingProviderService> _logger = logger;

        public async Task UpdateRegistration(ParkingProviderMessage registration, CancellationToken ct)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                // Call Machog Update service
                JsonContent content = JsonContent.Create(registration);
                HttpResponseMessage response = await _httpClient.PostAsync(string.Empty, content, ct);
                sw.Stop();

                _logger.LogInformation($"HTTP call to Machog took {sw.ElapsedMilliseconds} ms.");

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"==> {ex.Message}");
                throw;
            }
        }
    }
}
