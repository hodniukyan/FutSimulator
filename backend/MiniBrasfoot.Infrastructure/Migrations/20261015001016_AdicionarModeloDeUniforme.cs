using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniBrasfoot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModeloDeUniforme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModeloDoUniforme",
                table: "Teams",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeloDoUniforme",
                table: "Teams");
        }
    }
}
