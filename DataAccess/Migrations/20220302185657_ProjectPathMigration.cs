using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    public partial class ProjectPathMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastIndex",
                table: "Reviews",
                newName: "LastCharIndex");

            migrationBuilder.AddColumn<string>(
                name: "ProjectPath",
                table: "Reviews",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectPath",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "LastCharIndex",
                table: "Reviews",
                newName: "LastIndex");
        }
    }
}
