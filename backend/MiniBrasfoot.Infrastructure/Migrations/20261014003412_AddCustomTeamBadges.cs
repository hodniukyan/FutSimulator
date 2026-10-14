using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniBrasfoot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomTeamBadges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BadgeTemplate",
                table: "Teams",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsCustom",
                table: "Teams",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryColor",
                table: "Teams",
                type: "varchar(7)",
                maxLength: 7,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryColor",
                table: "Teams",
                type: "varchar(7)",
                maxLength: 7,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BadgeTemplate",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IsCustom",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "PrimaryColor",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "SecondaryColor",
                table: "Teams");
        }
    }
}
