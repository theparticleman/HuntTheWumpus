namespace HuntTheWumpus.Application.Services;

using HuntTheWumpus.Domain.Entities;
using HuntTheWumpus.Domain.Models;
using HuntTheWumpus.Application.Ports;
using System.Text.Json;

public interface IGameService
{
    Task<int> StartNewGameAsync(int playerId, int gridSize);
    Task<GameViewModel> GetGameByIdAsync(int gameId);
    Task<GameActionResult> MovePlayerAsync(int gameId, Direction direction);
    Task<GameActionResult> ShootArrowAsync(int gameId, Direction direction);
    Task<GameActionResult> PickUpGoldAsync(int gameId);
    Task<IEnumerable<HighScoreViewModel>> GetTopHighScoresAsync(int count);
}

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly IGameStateRepository _gameStateRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IHighScoreRepository _highScoreRepository;

    public GameService(
        IGameRepository gameRepository,
        IGameStateRepository gameStateRepository,
        IPlayerRepository playerRepository,
        IHighScoreRepository highScoreRepository)
    {
        _gameRepository = gameRepository;
        _gameStateRepository = gameStateRepository;
        _playerRepository = playerRepository;
        _highScoreRepository = highScoreRepository;
    }

    public async Task<int> StartNewGameAsync(int playerId, int gridSize)
    {
        var player = await _playerRepository.GetPlayerByIdAsync(playerId);
        if (player == null)
        {
            throw new ArgumentException("Player not found");
        }

        var game = new Game
        {
            PlayerId = playerId,
            StartTime = DateTime.UtcNow,
            Status = GameStatus.InProgress,
            Score = 0,
            GridSize = gridSize
        };

        int gameId = await _gameRepository.CreateGameAsync(game);

        // Create and initialize the game board
        var gameBoard = new GameBoard(gridSize);
        
        // Save initial state
        var gameState = new GameState
        {
            GameId = gameId,
            StateData = gameBoard.SerializeState(),
            Timestamp = DateTime.UtcNow
        };

        await _gameStateRepository.SaveGameStateAsync(gameState);

        return gameId;
    }

    public async Task<GameViewModel> GetGameByIdAsync(int gameId)
    {
        var game = await _gameRepository.GetGameByIdAsync(gameId);
        if (game == null)
        {
            throw new ArgumentException("Game not found");
        }

        var gameState = await _gameStateRepository.GetLatestStateForGameAsync(gameId);
        if (gameState == null)
        {
            throw new InvalidOperationException("Game state not found");
        }

        var gameBoard = GameBoard.DeserializeState(gameState.StateData) 
            ?? throw new InvalidOperationException("Could not deserialize game state");

        return new GameViewModel
        {
            GameId = game.GameId,
            PlayerId = game.PlayerId,
            Status = game.Status,
            Score = game.Score,
            GridSize = game.GridSize,
            Hints = gameBoard.GetHints(),
            HasArrow = gameBoard.HasArrow,
            HasGold = gameBoard.HasGold
        };
    }

    public async Task<GameActionResult> MovePlayerAsync(int gameId, Direction direction)
    {
        var game = await _gameRepository.GetGameByIdAsync(gameId);
        if (game == null || game.Status != GameStatus.InProgress)
        {
            throw new ArgumentException("Game not found or not in progress");
        }

        var gameState = await _gameStateRepository.GetLatestStateForGameAsync(gameId);
        if (gameState == null)
        {
            throw new InvalidOperationException("Game state not found");
        }

        var gameBoard = GameBoard.DeserializeState(gameState.StateData)
            ?? throw new InvalidOperationException("Could not deserialize game state");

        bool moveSuccessful = gameBoard.MovePlayer(direction);
        
        if (!moveSuccessful)
        {
            // Player died (Wumpus, pit)
            game.Status = GameStatus.Lost;
            game.EndTime = DateTime.UtcNow;
            await _gameRepository.UpdateGameAsync(game);
            
            return new GameActionResult
            {
                Success = false,
                Message = "You have died! Game over.",
                GameStatus = GameStatus.Lost
            };
        }
        
        // Save new game state
        var newGameState = new GameState
        {
            GameId = gameId,
            StateData = gameBoard.SerializeState(),
            Timestamp = DateTime.UtcNow
        };

        await _gameStateRepository.SaveGameStateAsync(newGameState);
        
        // Get new hints
        var hints = gameBoard.GetHints();
        
        return new GameActionResult
        {
            Success = true,
            Message = "Move successful",
            Hints = hints,
            GameStatus = GameStatus.InProgress,
            HasArrow = gameBoard.HasArrow,
            HasGold = gameBoard.HasGold
        };
    }

    public async Task<GameActionResult> ShootArrowAsync(int gameId, Direction direction)
    {
        var game = await _gameRepository.GetGameByIdAsync(gameId);
        if (game == null || game.Status != GameStatus.InProgress)
        {
            throw new ArgumentException("Game not found or not in progress");
        }

        var gameState = await _gameStateRepository.GetLatestStateForGameAsync(gameId);
        if (gameState == null)
        {
            throw new InvalidOperationException("Game state not found");
        }

        var gameBoard = GameBoard.DeserializeState(gameState.StateData)
            ?? throw new InvalidOperationException("Could not deserialize game state");

        if (!gameBoard.HasArrow)
        {
            return new GameActionResult
            {
                Success = false,
                Message = "You have no arrows left!",
                GameStatus = game.Status
            };
        }

        bool hitWumpus = gameBoard.ShootArrow(direction);
        
        // Save new game state regardless of outcome
        var newGameState = new GameState
        {
            GameId = gameId,
            StateData = gameBoard.SerializeState(),
            Timestamp = DateTime.UtcNow
        };

        await _gameStateRepository.SaveGameStateAsync(newGameState);
        
        if (hitWumpus)
        {
            game.Score += 500; // Bonus for killing the wumpus
            await _gameRepository.UpdateGameAsync(game);
            
            return new GameActionResult
            {
                Success = true,
                Message = "You killed the Wumpus!",
                GameStatus = game.Status,
                HasArrow = false,
                HasGold = gameBoard.HasGold
            };
        }
        
        return new GameActionResult
        {
            Success = true,
            Message = "You missed! You're out of arrows.",
            GameStatus = game.Status,
            HasArrow = false,
            HasGold = gameBoard.HasGold
        };
    }

    public async Task<GameActionResult> PickUpGoldAsync(int gameId)
    {
        var game = await _gameRepository.GetGameByIdAsync(gameId);
        if (game == null || game.Status != GameStatus.InProgress)
        {
            throw new ArgumentException("Game not found or not in progress");
        }

        var gameState = await _gameStateRepository.GetLatestStateForGameAsync(gameId);
        if (gameState == null)
        {
            throw new InvalidOperationException("Game state not found");
        }

        var gameBoard = GameBoard.DeserializeState(gameState.StateData)
            ?? throw new InvalidOperationException("Could not deserialize game state");

        if (gameBoard.HasGold)
        {
            // Player already has the gold, they must escape now
            return new GameActionResult
            {
                Success = false,
                Message = "You already have the gold! Escape the cave!",
                GameStatus = game.Status,
                HasGold = true
            };
        }
        
        // Check if player is on gold location
        if (gameBoard.PlayerLocation.Equals(gameBoard.GoldLocation))
        {
            gameBoard.HasGold = true;
            game.Score += 1000; // Bonus for getting the gold
            await _gameRepository.UpdateGameAsync(game);
            
            // Save new game state
            var newGameState = new GameState
            {
                GameId = gameId,
                StateData = gameBoard.SerializeState(),
                Timestamp = DateTime.UtcNow
            };

            await _gameStateRepository.SaveGameStateAsync(newGameState);
            
            return new GameActionResult
            {
                Success = true,
                Message = "You picked up the gold! Now escape the cave!",
                GameStatus = game.Status,
                HasGold = true
            };
        }
        
        return new GameActionResult
        {
            Success = false,
            Message = "There's no gold here to pick up.",
            GameStatus = game.Status,
            HasGold = false
        };
    }

    public async Task<IEnumerable<HighScoreViewModel>> GetTopHighScoresAsync(int count)
    {
        var highScores = await _highScoreRepository.GetTopHighScoresAsync(count);
        
        var result = new List<HighScoreViewModel>();
        
        foreach (var score in highScores)
        {
            var player = await _playerRepository.GetPlayerByIdAsync(score.PlayerId);
            if (player != null)
            {
                result.Add(new HighScoreViewModel
                {
                    PlayerName = player.Username,
                    Score = score.Score,
                    Date = score.GameDate
                });
            }
        }
        
        return result;
    }
}

public class GameViewModel
{
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public GameStatus Status { get; set; }
    public int Score { get; set; }
    public int GridSize { get; set; }
    public List<string> Hints { get; set; } = new List<string>();
    public bool HasArrow { get; set; }
    public bool HasGold { get; set; }
}

public class GameActionResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Hints { get; set; } = new List<string>();
    public GameStatus GameStatus { get; set; }
    public bool HasArrow { get; set; }
    public bool HasGold { get; set; }
}

public class HighScoreViewModel
{
    public string PlayerName { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime Date { get; set; }
}
