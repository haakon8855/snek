using Microsoft.AspNetCore.Identity;

namespace Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public int? HighScoreId { get; set; }

    public Score? HighScore { get; set; }

    public int StatsGamesStarted { get; set; }

    public int StatsTotalPoints { get; set; }

    public int StatsTotalInputs { get; set; }

    public int? Seed { get; set; }

    public DateTime? SeedTimestamp { get; set; }

    public double KillDeathRatio => StatsTotalPoints / (double)StatsGamesStarted;

    public double Efficiency => StatsTotalInputs / (double)StatsTotalPoints;
}