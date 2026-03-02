using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionPlanning.Migrations
{
    /// <inheritdoc />
    public partial class migration_29_01_2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatePlaningFinish",
                table: "ProductRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateInsert",
                table: "Orders",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateInsert",
                table: "Orders");

            migrationBuilder.AddColumn<DateTime>(
                name: "DatePlaningFinish",
                table: "ProductRequests",
                type: "datetime(6)",
                nullable: true);
        }
    }
}
