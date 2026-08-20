using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finder.Bot.Migrations
{
    /// <inheritdoc />
    public partial class moderation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Setting = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "UserLogs",
                columns: table => new
                {
                    GuildId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Bans = table.Column<int>(type: "integer", nullable: false),
                    Kicks = table.Column<int>(type: "integer", nullable: false),
                    Warns = table.Column<int>(type: "integer", nullable: false),
                    Mutes = table.Column<int>(type: "integer", nullable: false),
                    TempBan = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TempMute = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogs", x => new { x.GuildId, x.UserId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "UserLogs");
        }
    }
}
