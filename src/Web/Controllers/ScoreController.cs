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
public class ScoreController(GameService gameService) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> PublishScore([FromBody] HighScoreDTO points)
    {
        var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        if (points.Replay is null)
            return Results.BadRequest();

        var checkSum = await Game.Jsonify(points.Replay).ReadFromJsonAsync<HighScoreDTO>();

        if (checkSum?.Checksum != points.Checksum)
            return Results.BadRequest();

        ScoreSubmitResult result = await gameService.ValidateAndSubmitReplay(userId, points.Replay);

        return result switch
        {
            ScoreSubmitResult.HighScore => Results.Created(),
            ScoreSubmitResult.NotHighScore => Results.Ok(),
            ScoreSubmitResult.Invalid => Results.BadRequest("Could not validate high score"),
            _ => Results.BadRequest("Request parameters were ill formed")
        };
    }

    [HttpGet("{id:int}")]
    public async Task<IResult> GetScoreById(int id)
    {
        var score = await gameService.GetReplayByScoreId(id);

        if (score is null)
            return Results.BadRequest();

        return Results.Ok(score);
    }
}