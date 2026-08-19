using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Finder.Bot.Migrations
{
    /// <inheritdoc />
    public partial class changeticketing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClaimedUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "UserIds",
                table: "Tickets");

            migrationBuilder.CreateTable(
                name: "TicketClaimers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TicketChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketClaimers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketClaimers_Tickets_TicketChannelId",
                        column: x => x.TicketChannelId,
                        principalTable: "Tickets",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TicketChannelId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketUsers_Tickets_TicketChannelId",
                        column: x => x.TicketChannelId,
                        principalTable: "Tickets",
                        principalColumn: "ChannelId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketClaimers_TicketChannelId",
                table: "TicketClaimers",
                column: "TicketChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketUsers_TicketChannelId",
                table: "TicketUsers",
                column: "TicketChannelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketClaimers");

            migrationBuilder.DropTable(
                name: "TicketUsers");

            migrationBuilder.AddColumn<string>(
                name: "ClaimedUserId",
                table: "Tickets",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserIds",
                table: "Tickets",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }
    }
}
