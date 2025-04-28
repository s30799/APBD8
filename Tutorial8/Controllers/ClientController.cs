using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers
{
    [Route("api/clients")]
    [ApiController]
    public class ClientController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly ITripsService _tripsService;

        public ClientController(IClientService clientService, ITripsService tripsService)
        {
            _clientService = clientService;
            _tripsService = tripsService;
        }

        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            if(!await _clientService.DoesClientExistAsync(id))
                return NotFound($"Client with id {id} does not exist");
            var trips = await _clientService.GetClientTripsAsync(id);
            if(!trips.Any())
                return NotFound($"Client with id {id} is not registered for any trips");
            return Ok(trips);
        }

        [HttpPost]
        public async Task<IActionResult> AddClient([FromBody] NewClientDTO newClient)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            int newClientId = await _clientService.AddClientAsync(newClient);
            return CreatedAtAction(nameof(GetClientTrips), new { id = newClientId }, null);
        }

        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
        {
            if(!await _clientService.DoesClientExistAsync(id))
                return NotFound($"Client with id {id} does not exist");
            if(!await _tripsService.DoesTripExist(tripId.ToString()))
                return NotFound($"Trip with id {tripId} does not exist");
            if(await _clientService.IsClientRegisteredForTripAsync(id, tripId))
                return NotFound($"Trip with id {tripId} has already been registered");
            int currCount = await _tripsService.GetCurrendParticipantsCountAsync(tripId);
            int maxPpl = await _tripsService.GetMaxPeopleForTripAsync(tripId);
            if(currCount >= maxPpl)
                return BadRequest("Maximum number of participants for this trip is reached");
            await _clientService.RegisterClientForTripAsync(id, tripId);
            return Ok("Client registered for this trip successfully");
        }

        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> UnregisterClientForTrip(int id, int tripId)
        {
            if(!await _clientService.IsClientRegisteredForTripAsync(id, tripId))
                return NotFound($"Trip with id {tripId} has not been registered");
            await _clientService.UnregisterClientFromTripAsync(id, tripId);
            return Ok("Client unregistered for this trip successfully");
        }
    }
    
}
