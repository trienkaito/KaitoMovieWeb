using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesWebsite.Data.Migrations
{
    /// <inheritdoc />
    public partial class addImageMovie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Movie",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImageMovie",
                columns: table => new
                {
                    ImageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MovieId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageMovie", x => x.ImageId);
                    table.ForeignKey(
                        name: "FK_ImageMovie_Movie_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movie",
                        principalColumn: "MovieId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageMovie_MovieId",
                table: "ImageMovie",
                column: "MovieId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageMovie");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Movie");
        }
    }
}
