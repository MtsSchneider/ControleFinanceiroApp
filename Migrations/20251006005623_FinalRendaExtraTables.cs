using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiroApp.Migrations
{
    /// <inheritdoc />
    public partial class FinalRendaExtraTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GanhoUber",
                table: "GanhoUber");

            migrationBuilder.RenameTable(
                name: "GanhoUber",
                newName: "GanhosUber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GanhosUber",
                table: "GanhosUber",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_GanhosUber",
                table: "GanhosUber");

            migrationBuilder.RenameTable(
                name: "GanhosUber",
                newName: "GanhoUber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GanhoUber",
                table: "GanhoUber",
                column: "Id");
        }
    }
}
