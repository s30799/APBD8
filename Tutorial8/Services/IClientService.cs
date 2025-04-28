using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientService
{
    Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId);
    Task<bool> DoesClientExistAsync(int clientId);
    Task<int> AddClientAsync(NewClientDTO client);
    Task<bool> IsClientRegisteredForTripAsync(int clientId, int tripId);
    Task RegisterClientForTripAsync(int clientId, int tripId);
    Task UnregisterClientFromTripAsync(int clientId, int tripId);
}