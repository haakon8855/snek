using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Repositories;

public class GameRepository(ApplicationDbContext applicationDbContext)
{
    public async Task<Score?> GetHighScoreByUserId(string userId)
    {
        return await applicationDbContext.Scores
            .Where(s => s.User.Id == userId)
            .FirstOrDefaultAsync();
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
        
        await applicationDbContext.Scores.AddAsync(score);
        user.HighScore = score;
        await applicationDbContext.SaveChangesAsync();
    }

    public async Task<ApplicationUser> GetApplicationUser(string userId)
    {
        return await applicationDbContext.Users
            .Where(u => u.Id == userId)
            .FirstAsync();
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
}