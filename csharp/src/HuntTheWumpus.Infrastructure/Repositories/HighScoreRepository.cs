namespace HuntTheWumpus.Infrastructure.Repositories;

using Dapper;
using HuntTheWumpus.Application.Ports;
using HuntTheWumpus.Domain.Entities;

public class HighScoreRepository : IHighScoreRepository
{
    private readonly IDatabaseConnection _connection;
    
    public HighScoreRepository(IDatabaseConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<IEnumerable<HighScore>> GetTopHighScoresAsync(int count)
    {
        const string sql = @"
            SELECT * FROM HighScores
            ORDER BY Score DESC
            LIMIT @Count";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryAsync<HighScore>(sql, new { Count = count });
    }
    
    public async Task<IEnumerable<HighScore>> GetHighScoresByPlayerIdAsync(int playerId)
    {
        const string sql = @"
            SELECT * FROM HighScores
            WHERE PlayerId = @PlayerId
            ORDER BY Score DESC";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryAsync<HighScore>(sql, new { PlayerId = playerId });
    }
    
    public async Task<int> AddHighScoreAsync(HighScore highScore)
    {
        const string sql = @"
            INSERT INTO HighScores (PlayerId, Score, GameDate)
            VALUES (@PlayerId, @Score, @GameDate);
            SELECT LAST_INSERT_ID();";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.ExecuteScalarAsync<int>(sql, highScore);
    }
}
