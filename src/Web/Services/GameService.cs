using Web.Client.SnekLogic;
using Web.Data;
using Web.Models;
using Web.Repositories;

namespace Web.Services;

public class GameService(GameRepository gameRepository)
{
    public async Task<ApplicationUser?> GetUser(string userId)
    {
        return await gameRepository.GetApplicationUserWithHighScore(userId);
    }

    public async Task<int?> GetUserRank(string userId)
    {
        return await gameRepository.GetUserRankByUserId(userId);
    }
        
    public async Task<Score?> GetHighScoreByUserId(string userId)
    {
        var user = await gameRepository.GetApplicationUserWithHighScore(userId);
        return user.HighScore;
    }

    public async Task<Replay?> GetReplayByScoreId(int id)
    {
        var score = await gameRepository.GetScoreById(id);
        return score?.ReplayData;
    }

    public async Task DeleteScoresByUserId(string userId)
    {
        var user = await gameRepository.GetApplicationUser(userId);
        await gameRepository.DeleteScoresByUser(user);
    }

    public async Task<ScoreSubmitResult> SubmitScore(string userId, Replay replay)
    {
        var user = await gameRepository.GetApplicationUser(userId);

        var score = new Score
        {
            User = user,
            UserId = user.Id,
            Points = replay.Score,
            Timestamp = DateTime.UtcNow,
            ReplayData = replay
        };

        if (score.Points < 0)
            return ScoreSubmitResult.Failure;

        var currentHiScore = await gameRepository.GetHighScoreByUserId(user.Id);

        if (currentHiScore == null || currentHiScore.Points < score.Points)
        {
            await gameRepository.AddScoreAndSetHighScore(score);
            return ScoreSubmitResult.HighScore;
        }

        await gameRepository.AddScore(score);
        return ScoreSubmitResult.NotHighScore;
    }

    public async Task<List<Score>> GetTopScores(int amount)
    {
        return await gameRepository.GetTopScores(amount);
    }
}