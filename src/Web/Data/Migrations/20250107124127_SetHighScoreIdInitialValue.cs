using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SetHighScoreIdInitialValue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE AspNetUsers SET AspNetUsers.HighScoreId = (
                    SELECT TOP 1 Scores.Id
                    FROM Scores
                    WHERE Scores.UserId = AspNetUsers.Id
                    ORDER BY Scores.Points DESC
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
