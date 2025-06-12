namespace HuntTheWumpus.Infrastructure.Repositories;

using Dapper;
using HuntTheWumpus.Application.Ports;
using HuntTheWumpus.Domain.Entities;

public class GameStateRepository : IGameStateRepository
{
    private readonly IDatabaseConnection _connection;
    
    public GameStateRepository(IDatabaseConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<GameState?> GetLatestStateForGameAsync(int gameId)
    {
        const string sql = @"
            SELECT * FROM GameStates
            WHERE GameId = @GameId
            ORDER BY Timestamp DESC
            LIMIT 1";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryFirstOrDefaultAsync<GameState>(sql, new { GameId = gameId });
    }
    
    public async Task<int> SaveGameStateAsync(GameState gameState)
    {
        const string sql = @"
            INSERT INTO GameStates (GameId, StateData, Timestamp)
            VALUES (@GameId, @StateData, @Timestamp);
            SELECT LAST_INSERT_ID();";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.ExecuteScalarAsync<int>(sql, gameState);
    }
}
