namespace HuntTheWumpus.Application.Ports;

using HuntTheWumpus.Domain.Entities;

public interface IPlayerRepository
{
    Task<Player?> GetPlayerByIdAsync(int playerId);
    Task<Player?> GetPlayerByUsernameAsync(string username);
    Task<IEnumerable<Player>> GetAllPlayersAsync();
    Task<int> AddPlayerAsync(Player player);
    Task UpdatePlayerAsync(Player player);
}

public interface IGameRepository
{
    Task<Game?> GetGameByIdAsync(int gameId);
    Task<IEnumerable<Game>> GetGamesByPlayerIdAsync(int playerId);
    Task<int> CreateGameAsync(Game game);
    Task UpdateGameAsync(Game game);
}

public interface IGameStateRepository
{
    Task<GameState?> GetLatestStateForGameAsync(int gameId);
    Task<int> SaveGameStateAsync(GameState gameState);
}

public interface IHighScoreRepository
{
    Task<IEnumerable<HighScore>> GetTopHighScoresAsync(int count);
    Task<IEnumerable<HighScore>> GetHighScoresByPlayerIdAsync(int playerId);
    Task<int> AddHighScoreAsync(HighScore highScore);
}
