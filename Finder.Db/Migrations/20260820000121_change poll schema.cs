using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Finder.Db.Migrations
{
    /// <inheritdoc />
    public partial class changepollschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answers",
                table: "Polls");

            migrationBuilder.DropColumn(
                name: "VotersId",
                table: "Polls");

            migrationBuilder.CreateTable(
                name: "PollAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Answer = table.Column<string>(type: "text", nullable: false),
                    PollMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollAnswer_Polls_PollMessageId",
                        column: x => x.PollMessageId,
                        principalTable: "Polls",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PollVoter",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    PollMessageId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PollVoter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PollVoter_Polls_PollMessageId",
                        column: x => x.PollMessageId,
                        principalTable: "Polls",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PollAnswer_PollMessageId",
                table: "PollAnswer",
                column: "PollMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_PollVoter_PollMessageId",
                table: "PollVoter",
                column: "PollMessageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PollAnswer");

            migrationBuilder.DropTable(
                name: "PollVoter");

            migrationBuilder.AddColumn<List<string>>(
                name: "Answers",
                table: "Polls",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<decimal[]>(
                name: "VotersId",
                table: "Polls",
                type: "numeric(20,0)[]",
                nullable: false,
                defaultValue: new decimal[0]);
        }
    }
}
