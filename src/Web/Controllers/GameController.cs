using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Client.SnekLogic;
using Web.Models;
using Web.Services;
using System.Security.Claims;

namespace Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GameController(GameService gameService) : ControllerBase
{
    [HttpPost("score")]
    public async Task<IActionResult> Score([FromBody] HighScoreDTO points)
    {
        var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        if (points.Replay is null)
            return BadRequest();

        var checkSum = await Game.Jsonify(points.Replay).ReadFromJsonAsync<HighScoreDTO>();

        if (checkSum?.Checksum != points.Checksum)
            return BadRequest();

        if (!Game.VerifyReplay(points.Replay))
            return BadRequest();

        ScoreSubmitResult result = await gameService.SubmitScore(userId, points.Replay);

        return result switch
        {
            ScoreSubmitResult.HighScore => Created(),
            ScoreSubmitResult.NotHighScore => Ok(),
            ScoreSubmitResult.Failure => BadRequest(),
            _ => BadRequest()
        };
    }

    [HttpGet("score/{id:int}")]
    public async Task<IActionResult> Score(int id)
    {
        var score = await gameService.GetReplayByScoreId(id);
        if (score is null)
            return BadRequest();

        return Ok(score);
    }
}