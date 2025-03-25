using MachogPatch.Entities;

namespace MachogPatch.Services.ParkingProviderService
{
    public interface IParkingProviderService
    {
        public Task UpdateRegistration(ParkingProviderMessage registration, CancellationToken ct);
    }
}
