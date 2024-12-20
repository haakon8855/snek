﻿using Microsoft.AspNetCore.Authorization;
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
        string userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        if (points.Replay is null)
            return BadRequest();

        var checkSum = await Game.Jsonify(points.Replay).ReadFromJsonAsync<HighScoreDTO>();

        if (checkSum?.Checksum != points.Checksum)
            return BadRequest();

        if (!Game.VerifyReplay(points.Replay))
            return BadRequest();

        ScoreSubmitResult result = await gameService.SetScore(userId, points.Replay.Score);

        return result switch
        {
            ScoreSubmitResult.HighScore => Created(),
            ScoreSubmitResult.NotHighScore => Ok(),
            ScoreSubmitResult.Failure => BadRequest(),
            _ => BadRequest()
        };
    }
}