using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RockFS.Data.Migrations
{
    public partial class AddMounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HomeMount",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Mounts",
                columns: table => new
                {
                    MountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MountFriendlyName = table.Column<string>(type: "TEXT", nullable: false),
                    MountPath = table.Column<string>(type: "TEXT", nullable: false),
                    SizeLimit = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mounts", x => x.MountId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mounts");

            migrationBuilder.AddColumn<string>(
                name: "HomeMount",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
