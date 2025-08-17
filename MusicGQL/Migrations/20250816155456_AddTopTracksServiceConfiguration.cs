using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicGQL.Migrations
{
    /// <inheritdoc />
    public partial class AddTopTracksServiceConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LastFmTopTracksEnabled",
                table: "ServerSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ListenBrainzTopTracksEnabled",
                table: "ServerSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SpotifyTopTracksEnabled",
                table: "ServerSettings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastFmTopTracksEnabled",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "ListenBrainzTopTracksEnabled",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "SpotifyTopTracksEnabled",
                table: "ServerSettings");
        }
    }
}
