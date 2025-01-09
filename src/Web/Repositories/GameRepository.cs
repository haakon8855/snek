using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Repositories;

public class GameRepository(ApplicationDbContext applicationDbContext)
{
    public async Task<Score?> GetHighScoreByUserId(string userId)
    {
        var user = await applicationDbContext.Users
            .Include(u => u.HighScore)
            .FirstOrDefaultAsync(u => u.Id == userId);
        return user?.HighScore;
    }

    public async Task<int?> GetUserRankByUserId(string userId)
    {
        var user = await applicationDbContext.Users
            .Include(u => u.HighScore)
            .FirstOrDefaultAsync(u => u.Id == userId);
        var userScore = user?.HighScore?.Points;
        
        if (userScore is null)
            return null;

        var index = await applicationDbContext.Scores
            .Where(score => score.Points > userScore)
            .Include(score => score.User)
            .Join(applicationDbContext.Users,
                score => score.Id,
                user => user.HighScoreId,
                (score, user) => score)
            .CountAsync();
        return index + 1;
    }

    public async Task<Score?> GetScoreById(int id)
    {
        return await applicationDbContext.Scores
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteScoresByUser(ApplicationUser user)
    {
        var scores = applicationDbContext.Scores
            .Where(s => s.User.Id == user.Id);

        if (!scores.Any())
            return;

        applicationDbContext.Scores.RemoveRange(scores);
        await applicationDbContext.SaveChangesAsync();
    }

    public async Task AddScore(Score score)
    {
        var user = await applicationDbContext.Users
            .Include(u => u.HighScore)
            .FirstOrDefaultAsync(u => u.Id == score.UserId);

        if (user is null)
            return;

        // Update stats
        user.StatsGamesStarted += 1;
        user.StatsTotalPoints += score.Points;
        // If StatsTotalInputs has not yet been set it must be set for the first time
        if (user.StatsTotalInputs == 0)
            user.StatsTotalInputs = await GetInitalStatsTotalInputs(user);
        user.StatsTotalInputs += score.ReplayData?.Inputs.Count ?? 0;

        await applicationDbContext.Scores.AddAsync(score);
        await applicationDbContext.SaveChangesAsync();
    }

    public async Task AddScoreAndSetHighScore(Score score)
    {
        var user = await applicationDbContext.Users
            .Include(u => u.HighScore)
            .FirstOrDefaultAsync(u => u.Id == score.UserId);

        if (user is null)
            return;

        // Update stats
        user.StatsGamesStarted += 1;
        user.StatsTotalPoints += score.Points;
        // If StatsTotalInputs has not yet been set it must be set for the first time
        if (user.StatsTotalInputs == 0)
            user.StatsTotalInputs = await GetInitalStatsTotalInputs(user);
        user.StatsTotalInputs += score.ReplayData?.Inputs.Count ?? 0;

        // Update highscore
        user.HighScore = score;

        await applicationDbContext.Scores.AddAsync(score);
        await applicationDbContext.SaveChangesAsync();
    }

    private async Task<int> GetInitalStatsTotalInputs(ApplicationUser user)
    {
        // Get all stored scores for current user
        var allScoresForUser = await applicationDbContext.Scores
            .Where(s => s.UserId == user.Id)
            .ToListAsync();
        // Get total number of inputs for all these scores and store in user
        return allScoresForUser.Sum(s => s.ReplayData?.Inputs.Count ?? 0);
    }

    public async Task<ApplicationUser> GetApplicationUser(string userId)
    {
        var user = await applicationDbContext.Users
            .Where(u => u.Id == userId)
            .FirstAsync();

        if (user.StatsTotalInputs == 0)
        {
            user.StatsTotalInputs = await GetInitalStatsTotalInputs(user);
            await applicationDbContext.SaveChangesAsync();
        }

        return user;
    }

    public async Task<ApplicationUser> GetApplicationUserWithHighScore(string userId)
    {
        var user = await applicationDbContext.Users
            .Include(u => u.HighScore)
            .Where(u => u.Id == userId)
            .FirstAsync();

        if (user.StatsTotalInputs == 0)
        {
            user.StatsTotalInputs = await GetInitalStatsTotalInputs(user);
            await applicationDbContext.SaveChangesAsync();
        }

        return user;
    }

    public async Task<List<Score>> GetTopScores(int amount)
    {
        return await applicationDbContext.Scores
            .Include(score => score.User)
            .Join(applicationDbContext.Users,
                score => score.Id,
                user => user.HighScoreId,
                (score, user) => score)
            .OrderByDescending(s => s.Points)
            .Take(amount)
            .ToListAsync();
    }

    public async Task<List<Score>> GetRecentScoresByUserId(string userId, int amount)
    {
        return await applicationDbContext.Scores
            .Where(score => score.UserId == userId)
            .Include(score => score.User)
            .OrderByDescending(score => score.Timestamp)
            .Take(amount)
            .ToListAsync();
    }
}