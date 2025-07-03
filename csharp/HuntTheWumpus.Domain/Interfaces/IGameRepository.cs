using HuntTheWumpus.Domain.Entities;

namespace HuntTheWumpus.Domain.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);
    Task<Game?> GetActiveGameByPlayerIdAsync(string playerId);
    Task<IEnumerable<Game>> GetGamesByPlayerIdAsync(string playerId);
    Task SaveAsync(Game game);
    Task<IEnumerable<Game>> GetTopScoresAsync(int count = 10);
}
