using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartPantry.Migrations
{
    /// <inheritdoc />
    public partial class AddProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false),
                    Barcode = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: false),
                    Brand = table.Column<string>(type: "varchar(128)", maxLength: 128, nullable: true),
                    ExtraProperties = table.Column<string>(type: "longtext", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppProducts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppProducts");
        }
    }
}
