using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SetStatsInitialValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers SET AspNetUsers.StatsGamesStarted = (
                    SELECT Count(*)
                    FROM Scores
                    WHERE Scores.UserId = AspNetUsers.Id
                )               
            ");

            migrationBuilder.Sql(@"
                UPDATE AspNetUsers SET AspNetUsers.StatsTotalPoints = (
                    SELECT Sum(Points)
                    FROM Scores
                    WHERE Scores.UserId = AspNetUsers.Id
                )               
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
