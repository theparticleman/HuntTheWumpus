namespace HuntTheWumpus.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using HuntTheWumpus.Application.Services;
using HuntTheWumpus.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IGameService _gameService;
    
    public PlayersController(IGameService gameService)
    {
        _gameService = gameService;
    }
    
    [HttpGet("highscores")]
    public async Task<IActionResult> GetHighScores([FromQuery] int count = 10)
    {
        var highScores = await _gameService.GetTopHighScoresAsync(count);
        return Ok(highScores);
    }
}
