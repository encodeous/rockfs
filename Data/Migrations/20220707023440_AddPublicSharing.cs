using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RockFS.Data.Migrations
{
    public partial class AddPublicSharing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Mounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublicModifiable",
                table: "Mounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PublicId",
                table: "Mounts",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Mounts");

            migrationBuilder.DropColumn(
                name: "IsPublicModifiable",
                table: "Mounts");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Mounts");
        }
    }
}
