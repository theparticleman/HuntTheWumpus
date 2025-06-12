namespace HuntTheWumpus.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using HuntTheWumpus.Application.Services;
using HuntTheWumpus.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    
    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateGame([FromBody] CreateGameRequest request)
    {
        var gameId = await _gameService.StartNewGameAsync(request.PlayerId, request.GridSize);
        return CreatedAtAction(nameof(GetGame), new { id = gameId }, new { GameId = gameId });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(int id)
    {
        try
        {
            var game = await _gameService.GetGameByIdAsync(id);
            return Ok(game);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("{id}/move")]
    public async Task<IActionResult> MovePlayer(int id, [FromBody] MoveRequest request)
    {
        try
        {
            var result = await _gameService.MovePlayerAsync(id, request.Direction);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("{id}/shoot")]
    public async Task<IActionResult> ShootArrow(int id, [FromBody] MoveRequest request)
    {
        try
        {
            var result = await _gameService.ShootArrowAsync(id, request.Direction);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("{id}/pickup")]
    public async Task<IActionResult> PickUpGold(int id)
    {
        try
        {
            var result = await _gameService.PickUpGoldAsync(id);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class CreateGameRequest
{
    public int PlayerId { get; set; }
    public int GridSize { get; set; } = 10;
}

public class MoveRequest
{
    public Direction Direction { get; set; }
}
