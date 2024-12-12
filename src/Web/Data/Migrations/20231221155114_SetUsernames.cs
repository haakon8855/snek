using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class SetUsernames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = SUBSTRING(Email, 1, CHARINDEX('@', Email) - 1)");
            migrationBuilder.Sql("UPDATE AspNetUsers SET NormalizedUserName = UPPER(UserName)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE AspNetUsers SET UserName = Email");
            migrationBuilder.Sql("UPDATE AspNetUsers SET NormalizedUserName = LOWER(UserName)");
        }
    }
}
