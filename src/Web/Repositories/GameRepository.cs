using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Repositories;

public class GameRepository(ApplicationDbContext applicationDbContext)
{
    public async Task<Score?> GetScoreByUser(ApplicationUser user)
    {
        return await applicationDbContext.Scores
            .Where(s => s.User.Id == user.Id)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteScoreByUser(ApplicationUser user)
    {
        var score = await GetScoreByUser(user);
        if (score is null)
            return;

        applicationDbContext.Scores.Remove(score);
        await applicationDbContext.SaveChangesAsync();
    }

    public async Task SetNewHighScore(Score score)
    {
        var oldHiScore = await applicationDbContext.Scores
            .Where(s => s.User.Id == score.User.Id)
            .FirstOrDefaultAsync();

        if (oldHiScore != null)
            applicationDbContext.Scores.Remove(oldHiScore);

        await applicationDbContext.Scores.AddAsync(score);
        await applicationDbContext.SaveChangesAsync();
    }

    public async Task<ApplicationUser> GetApplicationUser(string userId)
    {
        return await applicationDbContext.Users.Where(u => u.Id == userId).FirstAsync();
    }

    public async Task<List<Score>> GetTopScores(int amount)
    {
        return await applicationDbContext.Scores
            .Include(s => s.User)
            .OrderByDescending(s => s.Points)
            .Take(amount)
            .ToListAsync();
    }
}