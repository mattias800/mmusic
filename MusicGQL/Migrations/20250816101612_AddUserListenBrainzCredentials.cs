using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicGQL.Migrations
{
    /// <inheritdoc />
    public partial class AddUserListenBrainzCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ListenBrainzToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListenBrainzUserId",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListenBrainzApiKey",
                table: "ServerSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ListenBrainzUsername",
                table: "ServerSettings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ListenBrainzToken",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListenBrainzUserId",
                table: "Events",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserListenBrainzCredentialsUpdated_SubjectUserId",
                table: "Events",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ListenBrainzToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ListenBrainzUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ListenBrainzApiKey",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "ListenBrainzUsername",
                table: "ServerSettings");

            migrationBuilder.DropColumn(
                name: "ListenBrainzToken",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ListenBrainzUserId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "UserListenBrainzCredentialsUpdated_SubjectUserId",
                table: "Events");
        }
    }
}
