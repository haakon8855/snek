using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Snek.Client.SnekLogic;
using Snek.Models;
using Snek.Services;
using System.Security.Claims;

namespace Snek.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GameController : ControllerBase
    {
        public GameController(GameService gameService)
        {
            GameService = gameService;
        }

        public GameService GameService { get; }

        [HttpPost("score")]
        public async Task<IActionResult> Score([FromBody] HighScoreDTO points)
        {
            string userId = User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).First().Value;

            if (points.Replay is null)
                return BadRequest();

            var checkSum = await Game.Jsonify(points.Replay).ReadFromJsonAsync<HighScoreDTO>();

            if (checkSum?.Checksum != points.Checksum)
                return BadRequest();

            if (!Game.VerifyReplay(points.Replay))
                return BadRequest();

            ScoreSubmitResult result = await GameService.SetScore(userId, points.Replay.Score);

            switch (result)
            {
                case ScoreSubmitResult.HighScore:
                    return Created();
                case ScoreSubmitResult.NotHighScore:
                    return Ok();
                case ScoreSubmitResult.Failure:
                    return BadRequest();
                default:
                    return BadRequest();
            }

        }
    }


}
