using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComBag.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceRangeToRepairService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedPriceRange",
                table: "RepairServices");

            migrationBuilder.DropColumn(
                name: "EstimatedTimeHours",
                table: "RepairServices");

            migrationBuilder.DropColumn(
                name: "StartingPrice",
                table: "RepairServices");

            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "RepairServices",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "PriceRange",
                table: "RepairServices",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceRange",
                table: "RepairServices");

            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "RepairServices",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedPriceRange",
                table: "RepairServices",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedTimeHours",
                table: "RepairServices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartingPrice",
                table: "RepairServices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
