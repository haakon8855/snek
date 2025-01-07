using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SetHighScoreInUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HighScoreId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HighScoreId",
                table: "AspNetUsers",
                column: "HighScoreId",
                unique: true,
                filter: "[HighScoreId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Scores_HighScoreId",
                table: "AspNetUsers",
                column: "HighScoreId",
                principalTable: "Scores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Scores_HighScoreId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HighScoreId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HighScoreId",
                table: "AspNetUsers");
        }
    }
}
