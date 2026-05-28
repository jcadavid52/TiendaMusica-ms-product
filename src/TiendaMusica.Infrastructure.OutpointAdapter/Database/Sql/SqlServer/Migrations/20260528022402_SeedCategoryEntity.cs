using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiendaMusica.Infrastructure.OutpointAdapter.Database.Sql.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class SeedCategoryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "ms-product",
                table: "Categories",
                columns: new[] { "Id", "CreationDateUtc", "Description", "Name" },
                values: new object[] { 1, new DateTime(2026, 5, 28, 2, 24, 2, 320, DateTimeKind.Utc).AddTicks(984), "Instrumentos musicales", "Instrumentos" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "ms-product",
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
