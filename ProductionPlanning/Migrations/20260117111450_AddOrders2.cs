using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProductionPlanning.Migrations
{
    /// <inheritdoc />
    public partial class AddOrders2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserCreaterId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserCreaterId",
                table: "Orders");
        }
    }
}
