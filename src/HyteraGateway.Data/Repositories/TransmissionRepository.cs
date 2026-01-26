using Dapper;
using HyteraGateway.Core.Models;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace HyteraGateway.Data.Repositories;

/// <summary>
/// Repository for managing transmission/call records in the database
/// </summary>
public class TransmissionRepository
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the TransmissionRepository
    /// </summary>
    /// <param name="configuration">Configuration containing connection string</param>
    public TransmissionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
    }

    /// <summary>
    /// Inserts a call record into the database
    /// </summary>
    /// <param name="call">Call record to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    public async Task<int> InsertAsync(CallRecord call, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            INSERT INTO dispatch_transmissions 
            (id, slot, radio_dmr_id, caller_alias, target_id, call_type, start_time, end_time, duration, audio_file_path, audio_file_size)
            VALUES 
            (@Id, @Slot, @CallerDmrId, @CallerAlias, @TargetId, @CallType, @StartTime, @EndTime, @Duration, @AudioFilePath, @AudioFileSize)";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.ExecuteAsync(new CommandDefinition(
            sql, 
            call, 
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Updates an existing call record
    /// </summary>
    /// <param name="call">Call record to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of rows affected</returns>
    public async Task<int> UpdateAsync(CallRecord call, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE dispatch_transmissions 
            SET end_time = @EndTime,
                duration = @Duration,
                audio_file_path = @AudioFilePath,
                audio_file_size = @AudioFileSize
            WHERE id = @Id";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.ExecuteAsync(new CommandDefinition(
            sql, 
            call, 
            cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Retrieves a call record by ID
    /// </summary>
    /// <param name="id">Call record ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Call record or null if not found</returns>
    public async Task<CallRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id as Id, slot as Slot, radio_dmr_id as CallerDmrId, caller_alias as CallerAlias,
                   target_id as TargetId, call_type as CallType, start_time as StartTime, 
                   end_time as EndTime, duration as Duration, audio_file_path as AudioFilePath,
                   audio_file_size as AudioFileSize
            FROM dispatch_transmissions 
            WHERE id = @Id";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<CallRecord>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Retrieves recent call records
    /// </summary>
    /// <param name="limit">Maximum number of records to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of recent call records</returns>
    public async Task<IEnumerable<CallRecord>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT id as Id, slot as Slot, radio_dmr_id as CallerDmrId, caller_alias as CallerAlias,
                   target_id as TargetId, call_type as CallType, start_time as StartTime, 
                   end_time as EndTime, duration as Duration, audio_file_path as AudioFilePath,
                   audio_file_size as AudioFileSize
            FROM dispatch_transmissions 
            ORDER BY start_time DESC 
            LIMIT @Limit";

        using var connection = new MySqlConnection(_connectionString);
        return await connection.QueryAsync<CallRecord>(
            new CommandDefinition(sql, new { Limit = limit }, cancellationToken: cancellationToken));
    }
}
