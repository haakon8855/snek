using Snek.Data;
using Snek.Models;
using Snek.Repositories;

namespace Snek.Services
{
    public class GameService
    {
        private readonly GameRepository _gameRepository;

        public GameService(GameRepository gameRepository)
        {
            _gameRepository = gameRepository;
        }


        public async Task<ApplicationUser> GetApplicationUser(string userId)
        {
            return await _gameRepository.GetApplicationUser(userId);
        }


        public async Task<Score?> GetScoreByUserId(string userId)
        {
            ApplicationUser user = await _gameRepository.GetApplicationUser(userId);
            return await _gameRepository.GetScoreByUser(user);
        }

        public async Task DeleteScoreByUserId(string userId)
        {
            ApplicationUser user = await _gameRepository.GetApplicationUser(userId);
            await _gameRepository.DeleteScoreByUser(user);
        }

        public async Task<ScoreSubmitResult> SetScore(string userId, int points)
        {
            ApplicationUser user = await _gameRepository.GetApplicationUser(userId);

            Score score = new Score
            {
                User = user,
                Points = points,
                Timestamp = DateTime.Now
            };

            if (score.Points < 0)
                return ScoreSubmitResult.Failure;

            var currentHiScore = await _gameRepository.GetScoreByUser(user);

            if (currentHiScore == null || currentHiScore.Points < score.Points)
            {
                await _gameRepository.SetNewHiScore(score);
                return ScoreSubmitResult.HighScore;
            }
            return ScoreSubmitResult.NotHighScore;
        }

        public async Task<List<Score>> GetTopScores(int amount)
        {
            return await _gameRepository.GetTopScores(amount);
        }
    }
}
