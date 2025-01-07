using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Web.Client.SnekLogic;

namespace Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Score> Scores { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var options = new JsonSerializerOptions();
        
        var jsonConverter = new ValueConverter<Replay?, string>(
            v => JsonSerializer.Serialize(v, options),
            v => JsonSerializer.Deserialize<Replay?>(v, options)
        );
        
        var jsonComparer = new ValueComparer<Replay?>(
            (d1, d2) => JsonSerializer.Serialize(d1, options) == JsonSerializer.Serialize(d2, options),
            d => JsonSerializer.Serialize(d, options).GetHashCode(),
            d => JsonSerializer.Deserialize<Replay?>(JsonSerializer.Serialize(d, options), options));
        
        builder.Entity<Score>()
            .Property(g => g.ReplayData)
            .HasConversion(jsonConverter)
            .Metadata.SetValueComparer(jsonComparer);

        builder.Entity<ApplicationUser>()
            .HasMany<Score>()
            .WithOne(score => score.User)
            .HasForeignKey(score => score.UserId);
        builder.Entity<Score>()
            .HasOne<ApplicationUser>()
            .WithOne(user => user.HighScore)
            .HasForeignKey<ApplicationUser>(user => user.HighScoreId);
    }
}