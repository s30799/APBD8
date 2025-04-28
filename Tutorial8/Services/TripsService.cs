using System.CodeDom;
using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=APBD;Integrated Security=True;";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new Dictionary<int, TripDTO>();

        string command = 
            @"
            SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                   c.Name AS CountryName
            FROM Trip t
                LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                LEFT JOIN Country c ON c.IdCountry = ct.IdCountry";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    if (!trips.ContainsKey(idTrip))
                    {
                        trips[idTrip] = new TripDTO()
                        {
                            IdTrip = idTrip,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                            DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                            MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                            Countries = new List<CountryDTO>()
                        };
                    }

                    if (!reader.IsDBNull(reader.GetOrdinal("CountryName")))
                    {
                        trips[idTrip].Countries.Add(new CountryDTO
                        {
                            Name = reader.GetString(reader.GetOrdinal("CountryName"))
                        });
                    }
                }
            }
        }
        return trips.Values.ToList();
    }

    public async Task<bool> DoesTripExist(string tripId)
    {
        string command = "SELECT 1 FROM Trip WHERE IdTrip = @IdTrip";
        
        using(SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }
    }

    public async Task<int> GetCurrendParticipantsCountAsync(int tripId)
    {
        string command = "Select Count(*) From Client_Trip WHERE IdTrip = @IdTrip";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync();
        } 
    }

    public async Task<int> GetMaxPeopleForTripAsync(int tripId)
    {
        string command = "Select MaxPeople From Trip WHERE IdTrip = @IdTrip";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@IdTrip", tripId);
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : throw new Exception("MaxPeople not found.");
        }
    }
}