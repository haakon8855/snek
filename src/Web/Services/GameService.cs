using Web.Client.SnekLogic;
using Web.Data;
using Web.Models;
using Web.Repositories;

namespace Web.Services;

public class GameService(GameRepository gameRepository)
{
    public async Task<Score?> GetScoreByUserId(string userId)
    {
        var user = await gameRepository.GetApplicationUser(userId);
        return await gameRepository.GetScoreByUser(user);
    }

    public async Task<Replay?> GetReplayById(int id)
    {
        var score = await gameRepository.GetScoreById(id);
        return score?.ReplayData;
    }

    public async Task DeleteScoreByUserId(string userId)
    {
        var user = await gameRepository.GetApplicationUser(userId);
        await gameRepository.DeleteScoreByUser(user);
    }

    public async Task<ScoreSubmitResult> SetScore(string userId, Replay replay)
    {
        var user = await gameRepository.GetApplicationUser(userId);

        var score = new Score
        {
            User = user,
            Points = replay.Score,
            Timestamp = DateTime.UtcNow,
            ReplayData = replay
        };

        if (score.Points < 0)
            return ScoreSubmitResult.Failure;

        var currentHiScore = await gameRepository.GetScoreByUser(user);

        if (currentHiScore == null || currentHiScore.Points < score.Points)
        {
            await gameRepository.SetNewHighScore(score);
            return ScoreSubmitResult.HighScore;
        }

        return ScoreSubmitResult.NotHighScore;
    }

    public async Task<List<Score>> GetTopScores(int amount)
    {
        return await gameRepository.GetTopScores(amount);
    }
}