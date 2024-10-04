using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Movie_MovieId",
                table: "History");

            migrationBuilder.RenameColumn(
                name: "MovieId",
                table: "History",
                newName: "EpisodeId");

            migrationBuilder.RenameIndex(
                name: "IX_History_MovieId_UserId_Time",
                table: "History",
                newName: "IX_History_EpisodeId_UserId_Time");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Episode_EpisodeId",
                table: "History",
                column: "EpisodeId",
                principalTable: "Episode",
                principalColumn: "EpisodeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Episode_EpisodeId",
                table: "History");

            migrationBuilder.RenameColumn(
                name: "EpisodeId",
                table: "History",
                newName: "MovieId");

            migrationBuilder.RenameIndex(
                name: "IX_History_EpisodeId_UserId_Time",
                table: "History",
                newName: "IX_History_MovieId_UserId_Time");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Movie_MovieId",
                table: "History",
                column: "MovieId",
                principalTable: "Movie",
                principalColumn: "MovieId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
