using Microsoft.AspNetCore.Mvc;
using HuntTheWumpus.Domain.Services;
using HuntTheWumpus.Domain.ValueObjects;

namespace HuntTheWumpus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly GameService _gameService;
    private readonly ILogger<GameController> _logger;

    public GameController(GameService gameService, ILogger<GameController> logger)
    {
        _gameService = gameService;
        _logger = logger;
    }

    [HttpPost("new")]
    public async Task<ActionResult<GameResponse>> CreateNewGame([FromBody] CreateGameRequest request)
    {
        try
        {
            var game = await _gameService.CreateNewGameAsync(request.PlayerId);
            var gameState = game.GetGameState();
            
            return Ok(new GameResponse
            {
                GameId = game.Id,
                Success = true,
                Message = "New game created successfully!",
                GameState = gameState
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new game for player {PlayerId}", request.PlayerId);
            return StatusCode(500, new GameResponse 
            { 
                Success = false, 
                Message = "An error occurred while creating the game." 
            });
        }
    }

    [HttpPost("{gameId}/move")]
    public async Task<ActionResult<GameResponse>> MovePlayer(Guid gameId, [FromBody] MoveRequest request)
    {
        try
        {
            var result = await _gameService.MovePlayerAsync(gameId, request.TargetRoom);
            
            return Ok(new GameResponse
            {
                GameId = gameId,
                Success = result.Success,
                Message = result.Message,
                GameState = result.GameState,
                IsGameOver = result.IsGameOver,
                IsVictory = result.IsVictory
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new GameResponse 
            { 
                Success = false, 
                Message = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving player in game {GameId}", gameId);
            return StatusCode(500, new GameResponse 
            { 
                Success = false, 
                Message = "An error occurred while processing the move." 
            });
        }
    }

    [HttpPost("{gameId}/shoot")]
    public async Task<ActionResult<GameResponse>> ShootArrow(Guid gameId, [FromBody] ShootRequest request)
    {
        try
        {
            var result = await _gameService.ShootArrowAsync(gameId, request.TargetRoom);
            
            return Ok(new GameResponse
            {
                GameId = gameId,
                Success = result.Success,
                Message = result.Message,
                GameState = result.GameState,
                IsGameOver = result.IsGameOver,
                IsVictory = result.IsVictory
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new GameResponse 
            { 
                Success = false, 
                Message = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shooting arrow in game {GameId}", gameId);
            return StatusCode(500, new GameResponse 
            { 
                Success = false, 
                Message = "An error occurred while processing the shot." 
            });
        }
    }

    [HttpGet("{gameId}/state")]
    public async Task<ActionResult<GameResponse>> GetGameState(Guid gameId)
    {
        try
        {
            var gameState = await _gameService.GetGameStateAsync(gameId);
            
            return Ok(new GameResponse
            {
                GameId = gameId,
                Success = true,
                Message = "Game state retrieved successfully.",
                GameState = gameState
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new GameResponse 
            { 
                Success = false, 
                Message = ex.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting game state for game {GameId}", gameId);
            return StatusCode(500, new GameResponse 
            { 
                Success = false, 
                Message = "An error occurred while retrieving the game state." 
            });
        }
    }

    [HttpGet("player/{playerId}/games")]
    public async Task<ActionResult<IEnumerable<GameHistoryResponse>>> GetPlayerGames(string playerId)
    {
        try
        {
            var games = await _gameService.GetPlayerGamesAsync(playerId);
            var response = games.Select(g => new GameHistoryResponse
            {
                GameId = g.Id,
                CreatedAt = g.CreatedAt,
                CompletedAt = g.CompletedAt,
                Status = g.Status.ToString(),
                Score = g.Score
            });
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting games for player {PlayerId}", playerId);
            return StatusCode(500, "An error occurred while retrieving player games.");
        }
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<IEnumerable<LeaderboardResponse>>> GetLeaderboard([FromQuery] int count = 10)
    {
        try
        {
            var games = await _gameService.GetLeaderboardAsync(count);
            var response = games.Select(g => new LeaderboardResponse
            {
                PlayerId = g.PlayerId,
                Score = g.Score,
                CompletedAt = g.CompletedAt!.Value
            });
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaderboard");
            return StatusCode(500, "An error occurred while retrieving the leaderboard.");
        }
    }
}

// DTOs
public class CreateGameRequest
{
    public string PlayerId { get; set; } = string.Empty;
}

public class MoveRequest
{
    public int TargetRoom { get; set; }
}

public class ShootRequest
{
    public int TargetRoom { get; set; }
}

public class GameResponse
{
    public Guid? GameId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public GameState? GameState { get; set; }
    public bool IsGameOver { get; set; }
    public bool IsVictory { get; set; }
}

public class GameHistoryResponse
{
    public Guid GameId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class LeaderboardResponse
{
    public string PlayerId { get; set; } = string.Empty;
    public int Score { get; set; }
    public DateTime CompletedAt { get; set; }
}
