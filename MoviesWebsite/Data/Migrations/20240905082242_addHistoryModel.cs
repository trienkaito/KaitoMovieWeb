using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class addHistoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episode_Movie_MovieId",
                table: "Episode");

            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    HistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MovieId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.HistoryId);
                    table.ForeignKey(
                        name: "FK_History_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_History_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_History_MovieId_UserId_Time",
                table: "History",
                columns: new[] { "MovieId", "UserId", "Time" });

            migrationBuilder.CreateIndex(
                name: "IX_History_UserId",
                table: "History",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episode_Movie_MovieId",
                table: "Episode",
                column: "MovieId",
                principalTable: "Movie",
                principalColumn: "MovieId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episode_Movie_MovieId",
                table: "Episode");

            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.AddForeignKey(
                name: "FK_Episode_Movie_MovieId",
                table: "Episode",
                column: "MovieId",
                principalTable: "Movie",
                principalColumn: "MovieId");
        }
    }
}
