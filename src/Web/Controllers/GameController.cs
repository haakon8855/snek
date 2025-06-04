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
    public async Task<IResult> Score([FromBody] HighScoreDTO points)
    {
        var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        if (points.Replay is null)
            return Results.BadRequest();

        var checkSum = await Game.Jsonify(points.Replay).ReadFromJsonAsync<HighScoreDTO>();

        if (checkSum?.Checksum != points.Checksum)
            return Results.BadRequest();

        if (!Game.VerifyReplay(points.Replay))
            return Results.BadRequest();

        ScoreSubmitResult result = await gameService.SubmitScore(userId, points.Replay);

        return result switch
        {
            ScoreSubmitResult.HighScore => Results.Created(),
            ScoreSubmitResult.NotHighScore => Results.Ok(),
            ScoreSubmitResult.Failure => Results.BadRequest("Could not save high score"),
            _ => Results.BadRequest("Request parameters were ill formed")
        };
    }

    [HttpGet("score/{id:int}")]
    public async Task<IResult> Score(int id)
    {
        var score = await gameService.GetReplayByScoreId(id);

        if (score is null)
            return Results.BadRequest();

        return Results.Ok(score);
    }
}