using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RockFS.Data.Migrations
{
    public partial class ApplyChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPublicModifiable",
                table: "Mounts",
                newName: "IsPublicWritable");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsPublicWritable",
                table: "Mounts",
                newName: "IsPublicModifiable");
        }
    }
}
