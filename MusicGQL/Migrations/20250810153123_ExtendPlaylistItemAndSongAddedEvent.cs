using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MusicGQL.Migrations
{
    /// <inheritdoc />
    public partial class ExtendPlaylistItemAndSongAddedEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServerArtists");

            migrationBuilder.DropTable(
                name: "ServerReleaseGroups");

            migrationBuilder.RenameColumn(
                name: "ReleaseGroupId",
                table: "Events",
                newName: "SongTitle");

            migrationBuilder.RenameColumn(
                name: "ArtistId",
                table: "Events",
                newName: "SongAddedToPlaylist_CoverImageUrl");

            migrationBuilder.AddColumn<string>(
                name: "ArtistName",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalAlbumId",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalArtistId",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalService",
                table: "PlaylistItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalTrackId",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalArtistId",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalCoverImageUrl",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalReleaseFolderName",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocalTrackNumber",
                table: "PlaylistItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseTitle",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseType",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SongTitle",
                table: "PlaylistItems",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrackLengthMs",
                table: "PlaylistItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArtistName",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalAlbumId",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalArtistId",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExternalService",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalTrackId",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalArtistId",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalCoverImageUrl",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalReleaseFolderName",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LocalTrackNumber",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseTitle",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseType",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrackLengthMs",
                table: "Events",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrackPlayCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ArtistId = table.Column<string>(type: "text", nullable: false),
                    ReleaseFolderName = table.Column<string>(type: "text", nullable: false),
                    TrackNumber = table.Column<int>(type: "integer", nullable: false),
                    ArtistName = table.Column<string>(type: "text", nullable: true),
                    ReleaseTitle = table.Column<string>(type: "text", nullable: true),
                    TrackTitle = table.Column<string>(type: "text", nullable: true),
                    PlayCount = table.Column<long>(type: "bigint", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackPlayCounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserTrackPlayCounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ArtistId = table.Column<string>(type: "text", nullable: false),
                    ReleaseFolderName = table.Column<string>(type: "text", nullable: false),
                    TrackNumber = table.Column<int>(type: "integer", nullable: false),
                    PlayCount = table.Column<long>(type: "bigint", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrackPlayCounts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrackPlayCounts_ArtistId_ReleaseFolderName_TrackNumber",
                table: "TrackPlayCounts",
                columns: new[] { "ArtistId", "ReleaseFolderName", "TrackNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTrackPlayCounts_UserId_ArtistId_ReleaseFolderName_Track~",
                table: "UserTrackPlayCounts",
                columns: new[] { "UserId", "ArtistId", "ReleaseFolderName", "TrackNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackPlayCounts");

            migrationBuilder.DropTable(
                name: "UserTrackPlayCounts");

            migrationBuilder.DropColumn(
                name: "ArtistName",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ExternalAlbumId",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ExternalArtistId",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ExternalService",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ExternalTrackId",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "LocalArtistId",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "LocalCoverImageUrl",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "LocalReleaseFolderName",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "LocalTrackNumber",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ReleaseTitle",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ReleaseType",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "SongTitle",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "TrackLengthMs",
                table: "PlaylistItems");

            migrationBuilder.DropColumn(
                name: "ArtistName",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ExternalAlbumId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ExternalArtistId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ExternalService",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ExternalTrackId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LocalArtistId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LocalCoverImageUrl",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LocalReleaseFolderName",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "LocalTrackNumber",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ReleaseTitle",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ReleaseType",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "TrackLengthMs",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "SongTitle",
                table: "Events",
                newName: "ReleaseGroupId");

            migrationBuilder.RenameColumn(
                name: "SongAddedToPlaylist_CoverImageUrl",
                table: "Events",
                newName: "ArtistId");

            migrationBuilder.CreateTable(
                name: "ServerArtists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ArtistId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerArtists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerArtists_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServerReleaseGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReleaseGroupId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerReleaseGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServerReleaseGroups_Users_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServerArtists_AddedByUserId",
                table: "ServerArtists",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerArtists_ArtistId",
                table: "ServerArtists",
                column: "ArtistId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServerReleaseGroups_AddedByUserId",
                table: "ServerReleaseGroups",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ServerReleaseGroups_ReleaseGroupId",
                table: "ServerReleaseGroups",
                column: "ReleaseGroupId",
                unique: true);
        }
    }
}
