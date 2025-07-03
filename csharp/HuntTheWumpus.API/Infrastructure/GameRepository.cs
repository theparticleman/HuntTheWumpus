using HuntTheWumpus.Domain.Entities;
using HuntTheWumpus.Domain.Interfaces;
using System.Collections.Concurrent;

namespace HuntTheWumpus.API.Infrastructure;

public class InMemoryGameRepository : IGameRepository
{
    private static readonly ConcurrentDictionary<Guid, Game> _games = new();

    public Task<Game?> GetByIdAsync(Guid id)
    {
        _games.TryGetValue(id, out var game);
        return Task.FromResult(game);
    }

    public Task<Game?> GetActiveGameByPlayerIdAsync(string playerId)
    {
        var activeGame = _games.Values
            .Where(g => g.PlayerId == playerId && g.Status == GameStatus.InProgress)
            .OrderByDescending(g => g.CreatedAt)
            .FirstOrDefault();
        
        return Task.FromResult(activeGame);
    }

    public Task<IEnumerable<Game>> GetGamesByPlayerIdAsync(string playerId)
    {
        var playerGames = _games.Values
            .Where(g => g.PlayerId == playerId)
            .OrderByDescending(g => g.CreatedAt)
            .AsEnumerable();
        
        return Task.FromResult(playerGames);
    }

    public Task SaveAsync(Game game)
    {
        _games.AddOrUpdate(game.Id, game, (key, oldValue) => game);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Game>> GetTopScoresAsync(int count = 10)
    {
        var topScores = _games.Values
            .Where(g => g.Status == GameStatus.Victory)
            .OrderByDescending(g => g.Score)
            .ThenBy(g => g.CompletedAt)
            .Take(count)
            .AsEnumerable();
        
        return Task.FromResult(topScores);
    }
}
