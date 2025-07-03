using HuntTheWumpus.Domain.Entities;
using HuntTheWumpus.Domain.Interfaces;
using HuntTheWumpus.Domain.ValueObjects;

namespace HuntTheWumpus.Domain.Services;

public class GameService
{
    private readonly IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<Game> CreateNewGameAsync(string playerId)
    {
        // End any existing active games for this player
        var existingGame = await _gameRepository.GetActiveGameByPlayerIdAsync(playerId);
        if (existingGame != null && existingGame.Status == GameStatus.InProgress)
        {
            // For simplicity, we'll allow multiple active games
            // In a real scenario, you might want to end the previous game
        }

        var game = new Game(playerId);
        await _gameRepository.SaveAsync(game);
        return game;
    }

    public async Task<GameMoveResult> MovePlayerAsync(Guid gameId, int targetRoom)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            throw new ArgumentException("Game not found", nameof(gameId));

        var result = game.MovePlayer(targetRoom);
        await _gameRepository.SaveAsync(game);
        return result;
    }

    public async Task<GameMoveResult> ShootArrowAsync(Guid gameId, int targetRoom)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            throw new ArgumentException("Game not found", nameof(gameId));

        var result = game.ShootArrow(targetRoom);
        await _gameRepository.SaveAsync(game);
        return result;
    }

    public async Task<GameState> GetGameStateAsync(Guid gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
            throw new ArgumentException("Game not found", nameof(gameId));

        return game.GetGameState();
    }

    public async Task<IEnumerable<Game>> GetPlayerGamesAsync(string playerId)
    {
        return await _gameRepository.GetGamesByPlayerIdAsync(playerId);
    }

    public async Task<IEnumerable<Game>> GetLeaderboardAsync(int count = 10)
    {
        return await _gameRepository.GetTopScoresAsync(count);
    }
}
