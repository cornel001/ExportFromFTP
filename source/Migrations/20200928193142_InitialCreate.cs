using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ExportFromFTP.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FilesInfo",
                columns: table => new
                {
                    Path = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    WriteTime = table.Column<DateTime>(nullable: false),
                    LastWriteTime = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilesInfo", x => x.Path);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilesInfo");
        }
    }
}
