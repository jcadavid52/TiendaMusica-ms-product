using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer.Migrations
{
    /// <inheritdoc />

    [ExcludeFromCodeCoverage]
    public partial class TPTStrategyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationDateUtc",
                schema: "ms-instruments",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "ms-instruments",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "ms-instruments",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "Price",
                schema: "ms-instruments",
                table: "Instruments");

            migrationBuilder.DropColumn(
                name: "Stock",
                schema: "ms-instruments",
                table: "Instruments");

            migrationBuilder.EnsureSchema(
                name: "ms-product");

            migrationBuilder.RenameTable(
                name: "Instruments",
                schema: "ms-instruments",
                newName: "Instruments",
                newSchema: "ms-product");

            migrationBuilder.CreateTable(
                name: "Categories",
                schema: "ms-product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreationDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "ms-product",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CreationDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "ms-product",
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                schema: "ms-product",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Instruments_Products_Id",
                schema: "ms-product",
                table: "Instruments",
                column: "Id",
                principalSchema: "ms-product",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instruments_Products_Id",
                schema: "ms-product",
                table: "Instruments");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "ms-product");

            migrationBuilder.DropTable(
                name: "Categories",
                schema: "ms-product");

            migrationBuilder.EnsureSchema(
                name: "ms-instruments");

            migrationBuilder.RenameTable(
                name: "Instruments",
                schema: "ms-product",
                newName: "Instruments",
                newSchema: "ms-instruments");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDateUtc",
                schema: "ms-instruments",
                table: "Instruments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "ms-instruments",
                table: "Instruments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "ms-instruments",
                table: "Instruments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                schema: "ms-instruments",
                table: "Instruments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                schema: "ms-instruments",
                table: "Instruments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
