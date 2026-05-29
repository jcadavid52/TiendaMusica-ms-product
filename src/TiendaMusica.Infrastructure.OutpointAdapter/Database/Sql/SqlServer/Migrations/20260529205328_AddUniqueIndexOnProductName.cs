using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexOnProductName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                schema: "ms-product",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreationDateUtc",
                value: new DateTime(2026, 5, 29, 20, 53, 27, 880, DateTimeKind.Utc).AddTicks(2771));

            migrationBuilder.CreateIndex(
                name: "IX_Products_Name",
                schema: "ms-product",
                table: "Products",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_Name",
                schema: "ms-product",
                table: "Products");

            migrationBuilder.UpdateData(
                schema: "ms-product",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreationDateUtc",
                value: new DateTime(2026, 5, 28, 2, 24, 2, 320, DateTimeKind.Utc).AddTicks(984));
        }
    }
}
