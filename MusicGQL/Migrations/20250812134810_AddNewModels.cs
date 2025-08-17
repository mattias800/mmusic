using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MusicGQL.Migrations
{
    /// <inheritdoc />
    public partial class AddNewModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventCheckpoints",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    LastProcessedEventId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventCheckpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    SubjectUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordingId = table.Column<string>(type: "text", nullable: true),
                    UnlikedSong_SubjectUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnlikedSong_RecordingId = table.Column<string>(type: "text", nullable: true),
                    PlaylistId = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    RenamedPlaylist_PlaylistId = table.Column<string>(type: "text", nullable: true),
                    NewPlaylistName = table.Column<string>(type: "text", nullable: true),
                    SongAddedToPlaylist_PlaylistId = table.Column<string>(type: "text", nullable: true),
                    PlaylistItemId = table.Column<string>(type: "text", nullable: true),
                    AtIndex = table.Column<int>(type: "integer", nullable: true),
                    LocalArtistId = table.Column<string>(type: "text", nullable: true),
                    LocalReleaseFolderName = table.Column<string>(type: "text", nullable: true),
                    LocalTrackNumber = table.Column<int>(type: "integer", nullable: true),
                    ExternalService = table.Column<int>(type: "integer", nullable: true),
                    ExternalTrackId = table.Column<string>(type: "text", nullable: true),
                    ExternalAlbumId = table.Column<string>(type: "text", nullable: true),
                    ExternalArtistId = table.Column<string>(type: "text", nullable: true),
                    SongTitle = table.Column<string>(type: "text", nullable: true),
                    ArtistName = table.Column<string>(type: "text", nullable: true),
                    ReleaseTitle = table.Column<string>(type: "text", nullable: true),
                    ReleaseType = table.Column<string>(type: "text", nullable: true),
                    TrackLengthMs = table.Column<int>(type: "integer", nullable: true),
                    SongAddedToPlaylist_CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    LocalCoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    NewPath = table.Column<string>(type: "text", nullable: true),
                    UserCreated_SubjectUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Username = table.Column<string>(type: "text", nullable: true),
                    UserPasswordHashUpdated_SubjectUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServerSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LibraryPath = table.Column<string>(type: "text", nullable: false),
                    DownloadPath = table.Column<string>(type: "text", nullable: false),
                    SoulSeekSearchTimeLimitSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServerSettings", x => x.Id);
                });

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
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
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

            migrationBuilder.CreateTable(
                name: "LikedSongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LikedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordingId = table.Column<string>(type: "text", nullable: false),
                    LikedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LikedSongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LikedSongs_Users_LikedByUserId",
                        column: x => x.LikedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    PlaylistId = table.Column<string>(type: "text", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LocalArtistId = table.Column<string>(type: "text", nullable: true),
                    LocalReleaseFolderName = table.Column<string>(type: "text", nullable: true),
                    LocalTrackNumber = table.Column<int>(type: "integer", nullable: true),
                    ExternalService = table.Column<int>(type: "integer", nullable: true),
                    ExternalTrackId = table.Column<string>(type: "text", nullable: true),
                    ExternalAlbumId = table.Column<string>(type: "text", nullable: true),
                    ExternalArtistId = table.Column<string>(type: "text", nullable: true),
                    SongTitle = table.Column<string>(type: "text", nullable: true),
                    ArtistName = table.Column<string>(type: "text", nullable: true),
                    ReleaseTitle = table.Column<string>(type: "text", nullable: true),
                    ReleaseType = table.Column<string>(type: "text", nullable: true),
                    TrackLengthMs = table.Column<int>(type: "integer", nullable: true),
                    CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                    LocalCoverImageUrl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistItems_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LikedSongs_LikedByUserId",
                table: "LikedSongs",
                column: "LikedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LikedSongs_LikedByUserId_RecordingId",
                table: "LikedSongs",
                columns: new[] { "LikedByUserId", "RecordingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistItems_PlaylistId",
                table: "PlaylistItems",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists",
                column: "UserId");

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
                name: "EventCheckpoints");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "LikedSongs");

            migrationBuilder.DropTable(
                name: "PlaylistItems");

            migrationBuilder.DropTable(
                name: "ServerSettings");

            migrationBuilder.DropTable(
                name: "TrackPlayCounts");

            migrationBuilder.DropTable(
                name: "UserTrackPlayCounts");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
