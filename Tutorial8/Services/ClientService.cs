using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientService : IClientService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";

    public async Task<IEnumerable<ClientTripDTO>> GetClientTripsAsync(int clientId)
    {
        var trips = new List<ClientTripDTO>();
        string command = @"SELECT ct.IdClient, ct.IdTrip, t.Name AS TripName, t.DateFrom, t.DateTo, ct.RegisteredAt, ct.PaymentDate
                            FROM Client_Trip ct
                            JOIN Trip t ON t.IdTrip = ct.IdTrip
                            WHERE ct.IdClient = @IdClient";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(command, conn))
            {
                await conn.OpenAsync();
                cmd.Parameters.AddWithValue("@IdClient", clientId);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        trips.Add(new ClientTripDTO
                        {
                            IdClient = reader.GetInt32(0),
                            IdTrip = reader.GetInt32(1),
                            TripName = reader.GetString(2),
                            DateFrom = reader.GetDateTime(3),
                            DateTo = reader.GetDateTime(4),
                            RegisteredAt = reader.GetDateTime(5),
                            PaymentDate = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                        });
                    }
                }
            }
            return trips;
    }

    public async Task<bool> DoesClientExistAsync(int clientId)
    {
        string command = "SELECT 1 FROM Client WHERE IdClient = @IdClient";
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@IdClient", clientId);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
    }

    public async Task<int> AddClientAsync(NewClientDTO client)
    {
        string command =  @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
                                 OUTPUT INSERTED.IdClient
                                 VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)";
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("@LastName", client.LastName);
            cmd.Parameters.AddWithValue("@Email", client.Email);
            cmd.Parameters.AddWithValue("@Telephone", client.Phone);
            cmd.Parameters.AddWithValue("@Pesel", client.Pesel);
            await conn.OpenAsync();
            
            return (int)await cmd.ExecuteScalarAsync();
        } 
    }

    public async Task<bool> IsClientRegisteredForTripAsync(int clientId, int tripId)
    {
        string command = "SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@IdClient", clientId);
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }  
    }

    public async Task RegisterClientForTripAsync(int clientId, int tripId)
    {
        string command = @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                                 VALUES (@IdClient, @IdTrip, @RegisteredAt)";
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            if(await IsClientRegisteredForTripAsync(clientId, tripId))
                throw new Exception("Client is already registered for this trip");
            cmd.Parameters.AddWithValue("@IdClient", clientId);
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            cmd.Parameters.AddWithValue("@RegisteredAt", DateTime.Now);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }   
    }

    public async Task UnregisterClientFromTripAsync(int clientId, int tripId)
    {
        string command = "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            if(!await IsClientRegisteredForTripAsync(clientId, tripId))
                throw new Exception("Client is not registered");
            cmd.Parameters.AddWithValue("@IdClient", clientId);
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }    
    }
}