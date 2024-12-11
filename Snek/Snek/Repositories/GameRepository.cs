using Microsoft.EntityFrameworkCore;
using Snek.Data;

namespace Snek.Repositories
{
    public class GameRepository
    {
        private readonly ApplicationDbContext _context;
        public GameRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }



        public async Task<Score?> GetScoreByUser(ApplicationUser user)
        {
            return await _context.Scores.Where(s => s.User.Id == user.Id).FirstOrDefaultAsync();
        }

        public async Task DeleteScoreByUser(ApplicationUser user)
        {
            Score? score = await GetScoreByUser(user);
            if (score is null)
            {
                return;
            }
            _context.Scores.Remove(score);
            await _context.SaveChangesAsync();
        }

        public async Task SetNewHiScore(Score score)
        {
            var oldHiScore = await _context.Scores.Where(s => s.User.Id == score.User.Id).FirstOrDefaultAsync();
            if (oldHiScore != null)
                _context.Scores.Remove(oldHiScore);
            await _context.Scores.AddAsync(score);
            await _context.SaveChangesAsync();
        }

        public async Task<ApplicationUser> GetApplicationUser(string userId)
        {
            return await _context.Users.Where(u => u.Id == userId).FirstAsync();
        }

        public async Task<List<Score>> GetTopScores(int amount)
        {
            return await _context.Scores.Include(s => s.User).OrderByDescending(s => s.Points).Take(amount).ToListAsync();
        }
    }
}
