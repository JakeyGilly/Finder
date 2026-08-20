using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finder.Db.Migrations
{
    /// <inheritdoc />
    public partial class forgottables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InventoryModel",
                table: "InventoryModel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EconomyModel",
                table: "EconomyModel");

            migrationBuilder.RenameTable(
                name: "InventoryModel",
                newName: "Inventory");

            migrationBuilder.RenameTable(
                name: "EconomyModel",
                newName: "Economy");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory",
                columns: new[] { "GuildId", "UserId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Economy",
                table: "Economy",
                columns: new[] { "GuildId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventory",
                table: "Inventory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Economy",
                table: "Economy");

            migrationBuilder.RenameTable(
                name: "Inventory",
                newName: "InventoryModel");

            migrationBuilder.RenameTable(
                name: "Economy",
                newName: "EconomyModel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InventoryModel",
                table: "InventoryModel",
                columns: new[] { "GuildId", "UserId", "ItemId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_EconomyModel",
                table: "EconomyModel",
                columns: new[] { "GuildId", "UserId" });
        }
    }
}
