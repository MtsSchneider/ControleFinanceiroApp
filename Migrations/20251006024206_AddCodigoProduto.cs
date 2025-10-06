using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControleFinanceiroApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCodigoProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoProduto",
                table: "Produtos",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoProduto",
                table: "Produtos");
        }
    }
}
