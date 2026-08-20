using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finder.Db.Migrations
{
    /// <inheritdoc />
    public partial class updatepolltables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollAnswer_Polls_PollMessageId",
                table: "PollAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_PollVoter_Polls_PollMessageId",
                table: "PollVoter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PollVoter",
                table: "PollVoter");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PollAnswer",
                table: "PollAnswer");

            migrationBuilder.RenameTable(
                name: "PollVoter",
                newName: "PollVoters");

            migrationBuilder.RenameTable(
                name: "PollAnswer",
                newName: "PollAnswers");

            migrationBuilder.RenameIndex(
                name: "IX_PollVoter_PollMessageId",
                table: "PollVoters",
                newName: "IX_PollVoters_PollMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_PollAnswer_PollMessageId",
                table: "PollAnswers",
                newName: "IX_PollAnswers_PollMessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PollVoters",
                table: "PollVoters",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PollAnswers",
                table: "PollAnswers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PollAnswers_Polls_PollMessageId",
                table: "PollAnswers",
                column: "PollMessageId",
                principalTable: "Polls",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PollVoters_Polls_PollMessageId",
                table: "PollVoters",
                column: "PollMessageId",
                principalTable: "Polls",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PollAnswers_Polls_PollMessageId",
                table: "PollAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_PollVoters_Polls_PollMessageId",
                table: "PollVoters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PollVoters",
                table: "PollVoters");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PollAnswers",
                table: "PollAnswers");

            migrationBuilder.RenameTable(
                name: "PollVoters",
                newName: "PollVoter");

            migrationBuilder.RenameTable(
                name: "PollAnswers",
                newName: "PollAnswer");

            migrationBuilder.RenameIndex(
                name: "IX_PollVoters_PollMessageId",
                table: "PollVoter",
                newName: "IX_PollVoter_PollMessageId");

            migrationBuilder.RenameIndex(
                name: "IX_PollAnswers_PollMessageId",
                table: "PollAnswer",
                newName: "IX_PollAnswer_PollMessageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PollVoter",
                table: "PollVoter",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PollAnswer",
                table: "PollAnswer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PollAnswer_Polls_PollMessageId",
                table: "PollAnswer",
                column: "PollMessageId",
                principalTable: "Polls",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PollVoter_Polls_PollMessageId",
                table: "PollVoter",
                column: "PollMessageId",
                principalTable: "Polls",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
