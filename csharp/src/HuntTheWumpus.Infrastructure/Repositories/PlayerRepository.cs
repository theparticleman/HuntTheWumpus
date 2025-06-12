namespace HuntTheWumpus.Infrastructure.Repositories;

using Dapper;
using HuntTheWumpus.Application.Ports;
using HuntTheWumpus.Domain.Entities;

public class PlayerRepository : IPlayerRepository
{
    private readonly IDatabaseConnection _connection;
    
    public PlayerRepository(IDatabaseConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<Player?> GetPlayerByIdAsync(int playerId)
    {
        const string sql = "SELECT * FROM Players WHERE PlayerId = @PlayerId";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryFirstOrDefaultAsync<Player>(sql, new { PlayerId = playerId });
    }
    
    public async Task<Player?> GetPlayerByUsernameAsync(string username)
    {
        const string sql = "SELECT * FROM Players WHERE Username = @Username";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryFirstOrDefaultAsync<Player>(sql, new { Username = username });
    }
    
    public async Task<IEnumerable<Player>> GetAllPlayersAsync()
    {
        const string sql = "SELECT * FROM Players";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryAsync<Player>(sql);
    }
    
    public async Task<int> AddPlayerAsync(Player player)
    {
        const string sql = @"
            INSERT INTO Players (Username, Email, RegistrationDate, LastLoginDate)
            VALUES (@Username, @Email, @RegistrationDate, @LastLoginDate);
            SELECT LAST_INSERT_ID();";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.ExecuteScalarAsync<int>(sql, player);
    }
    
    public async Task UpdatePlayerAsync(Player player)
    {
        const string sql = @"
            UPDATE Players
            SET Username = @Username,
                Email = @Email,
                LastLoginDate = @LastLoginDate
            WHERE PlayerId = @PlayerId";
        
        using var dbConnection = _connection.CreateConnection();
        await dbConnection.ExecuteAsync(sql, player);
    }
}
