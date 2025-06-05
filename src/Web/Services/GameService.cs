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

    public async Task<List<Score>> GetRecentScores(string userId, int amount)
    {
        return await gameRepository.GetRecentScoresByUserId(userId, amount);
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

    public async Task<ScoreSubmitResult> ValidateAndSubmitReplay(string userId, Replay replay)
    {
        // Check if reported score is negative (highly unlikely)
        if (replay.Score < 0)
            return ScoreSubmitResult.Invalid;

        // Get user
        var user = await gameRepository.GetApplicationUser(userId);

        // Check if seed was generated
        if (user.Seed is null || user.SeedTimestamp is null)
            return ScoreSubmitResult.Invalid;

        // Check if submitted seed is equal to generated seed
        if (replay.Seed != user.Seed)
            return ScoreSubmitResult.Invalid;

        // Check if inputs and seed will reproduce reported score
        if (!(await Game.ValidateReplay(replay)))
            return ScoreSubmitResult.Invalid;

        // Everything checks out, create score and store in database
        var score = new Score
        {
            User = user,
            UserId = user.Id,
            Points = replay.Score,
            Timestamp = DateTime.UtcNow,
            ReplayData = replay
        };

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

    public async Task<int?> StartGameForUser(string userId)
    {
        return await gameRepository.StartGameForUser(userId);
    }
}