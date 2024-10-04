using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class UodateMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Trailer",
                table: "Movie",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Trailer",
                table: "Movie");
        }
    }
}
