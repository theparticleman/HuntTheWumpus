namespace HuntTheWumpus.Infrastructure.Repositories;

using Dapper;
using HuntTheWumpus.Application.Ports;
using HuntTheWumpus.Domain.Entities;

public class GameRepository : IGameRepository
{
    private readonly IDatabaseConnection _connection;
    
    public GameRepository(IDatabaseConnection connection)
    {
        _connection = connection;
    }
    
    public async Task<Game?> GetGameByIdAsync(int gameId)
    {
        const string sql = @"
            SELECT * FROM Games
            WHERE GameId = @GameId";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryFirstOrDefaultAsync<Game>(sql, new { GameId = gameId });
    }
    
    public async Task<IEnumerable<Game>> GetGamesByPlayerIdAsync(int playerId)
    {
        const string sql = @"
            SELECT * FROM Games
            WHERE PlayerId = @PlayerId
            ORDER BY StartTime DESC";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.QueryAsync<Game>(sql, new { PlayerId = playerId });
    }
    
    public async Task<int> CreateGameAsync(Game game)
    {
        const string sql = @"
            INSERT INTO Games (PlayerId, StartTime, Status, Score, GridSize)
            VALUES (@PlayerId, @StartTime, @Status, @Score, @GridSize);
            SELECT LAST_INSERT_ID();";
        
        using var dbConnection = _connection.CreateConnection();
        return await dbConnection.ExecuteScalarAsync<int>(sql, new
        {
            game.PlayerId,
            game.StartTime,
            Status = game.Status.ToString(),
            game.Score,
            game.GridSize
        });
    }
    
    public async Task UpdateGameAsync(Game game)
    {
        const string sql = @"
            UPDATE Games
            SET EndTime = @EndTime,
                Status = @Status,
                Score = @Score
            WHERE GameId = @GameId";
        
        using var dbConnection = _connection.CreateConnection();
        await dbConnection.ExecuteAsync(sql, new
        {
            game.GameId,
            game.EndTime,
            Status = game.Status.ToString(),
            game.Score
        });
    }
}
