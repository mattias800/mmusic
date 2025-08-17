using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicGQL.Migrations
{
    /// <inheritdoc />
    public partial class AddSoulSeekBatchDownloading : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SoulSeekBatchDownloadingEnabled",
                table: "ServerSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoulSeekBatchDownloadingEnabled",
                table: "ServerSettings");
        }
    }
}
