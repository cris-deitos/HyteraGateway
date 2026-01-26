using Dapper;
using HyteraGateway.Core.Models;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace HyteraGateway.Data.Repositories;

/// <summary>
/// Repository for managing GPS position records in the database
/// </summary>
public class PositionRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the PositionRepository
    /// </summary>
    /// <param name="configuration">Configuration containing connection string</param>
    public PositionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    }

    /// <summary>
    /// Inserts a GPS position record into the database
    /// </summary>
    /// <param name="position">GPS position to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    public async Task<int> InsertAsync(GpsPosition position, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dispatch_positions 
            (id, radio_dmr_id, timestamp, latitude, longitude, altitude, speed, heading, accuracy)
            VALUES 
            (@Id, @RadioDmrId, @Timestamp, @Latitude, @Longitude, @Altitude, @Speed, @Heading, @Accuracy)";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.ExecuteAsync(new CommandDefinition(
            sql, 
            position, 
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Retrieves the latest position for a specific radio
    /// </summary>
    /// <param name="radioDmrId">DMR ID of the radio</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Latest GPS position or null if not found</returns>
    public async Task<GpsPosition?> GetLatestByRadioAsync(int radioDmrId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id as Id, radio_dmr_id as RadioDmrId, timestamp as Timestamp,
                   latitude as Latitude, longitude as Longitude, altitude as Altitude,
                   speed as Speed, heading as Heading, accuracy as Accuracy
            FROM dispatch_positions 
            WHERE radio_dmr_id = @RadioDmrId 
            ORDER BY timestamp DESC 
            LIMIT 1";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<GpsPosition>(
            new CommandDefinition(sql, new { RadioDmrId = radioDmrId }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Retrieves position history for a specific radio
    /// </summary>
    /// <param name="radioDmrId">DMR ID of the radio</param>
    /// <param name="since">Only return positions after this timestamp</param>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of GPS positions</returns>
    public async Task<IEnumerable<GpsPosition>> GetHistoryAsync(
        int radioDmrId, 
        DateTime? since = null, 
        int limit = 100, 
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT id as Id, radio_dmr_id as RadioDmrId, timestamp as Timestamp,
                   latitude as Latitude, longitude as Longitude, altitude as Altitude,
                   speed as Speed, heading as Heading, accuracy as Accuracy
            FROM dispatch_positions 
            WHERE radio_dmr_id = @RadioDmrId";

        if (since.HasValue)
        {
            sql += " AND timestamp >= @Since";
        }

        sql += " ORDER BY timestamp DESC LIMIT @Limit";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync<GpsPosition>(
            new CommandDefinition(
                sql, 
                new { RadioDmrId = radioDmrId, Since = since, Limit = limit }, 
                cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Retrieves all latest positions for all radios
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of latest positions grouped by radio</returns>
    public async Task<IEnumerable<GpsPosition>> GetAllLatestAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT p1.id as Id, p1.radio_dmr_id as RadioDmrId, p1.timestamp as Timestamp,
                   p1.latitude as Latitude, p1.longitude as Longitude, p1.altitude as Altitude,
                   p1.speed as Speed, p1.heading as Heading, p1.accuracy as Accuracy
            FROM dispatch_positions p1
            INNER JOIN (
                SELECT radio_dmr_id, MAX(timestamp) as max_timestamp
                FROM dispatch_positions
                GROUP BY radio_dmr_id
            ) p2 ON p1.radio_dmr_id = p2.radio_dmr_id AND p1.timestamp = p2.max_timestamp";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync<GpsPosition>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}
