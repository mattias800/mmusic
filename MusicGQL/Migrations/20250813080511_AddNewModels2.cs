using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicGQL.Migrations
{
    /// <inheritdoc />
    public partial class AddNewModels2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SoulSeekNoDataTimeoutSeconds",
                table: "ServerSettings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoulSeekNoDataTimeoutSeconds",
                table: "ServerSettings");
        }
    }
}
