using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionPlanning.Migrations
{
    /// <inheritdoc />
    public partial class Migration_19_01_2026_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_ProductRequests_ProductRequestId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ProductRequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductRequestId",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProductRequestId",
                table: "Orders",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ProductRequestId",
                table: "Orders",
                column: "ProductRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_ProductRequests_ProductRequestId",
                table: "Orders",
                column: "ProductRequestId",
                principalTable: "ProductRequests",
                principalColumn: "Id");
        }
    }
}
