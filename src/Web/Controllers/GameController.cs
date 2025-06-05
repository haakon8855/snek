using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Client.SnekLogic;
using Web.Services;

namespace Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class GameController(GameService gameService, ILogger<GameController> logger) : ControllerBase
{
    [HttpPost("start")]
    public async Task<IResult> StartGame()
    {
        var userId = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        
        logger.LogInformation($"Generating seed and timestamp for user {userId}");

        var seed = await gameService.StartGameForUser(userId) ?? 0;
        
        if (seed == 0)
            return Results.NotFound("User does not exist");

        var responseBody = new SeedDTO { Seed = seed };
        return Results.Ok(responseBody);
    }
}