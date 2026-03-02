using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductionPlanning.Migrations
{
    /// <inheritdoc />
    public partial class _21_02_2026_2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "А6-04");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "А6-06");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "А16-512");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Type" },
                values: new object[] { "А6-04 + адаптер GSM", "Device" });

            migrationBuilder.InsertData(
                table: "Equipments",
                columns: new[] { "Id", "Article", "Name", "Type" },
                values: new object[,]
                {
                    { 5, "", "А6-06 + адаптер GSM", "Device" },
                    { 6, "", "А6-512 + адаптер GSM", "Device" },
                    { 7, "", "АМС-8", "Device" },
                    { 8, "", "РМ-68-2", "Device" },
                    { 9, "", "SIM800 1 - сим", "Device" },
                    { 10, "", "РМ-64", "Device" },
                    { 11, "", "ИС-485", "Device" },
                    { 12, "", "ИС-USB", "Device" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "A24");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "A16");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Бирюза");

            migrationBuilder.UpdateData(
                table: "Equipments",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "Type" },
                values: new object[] { "Модуль GSM", "Components" });
        }
    }
}
