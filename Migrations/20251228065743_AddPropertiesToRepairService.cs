using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComBag.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertiesToRepairService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepairServiceId1",
                table: "ServiceInquiries",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedTimeHours",
                table: "RepairServices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "RepairServices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartingPrice",
                table: "RepairServices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceInquiries_RepairServiceId1",
                table: "ServiceInquiries",
                column: "RepairServiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceInquiries_RepairServices_RepairServiceId1",
                table: "ServiceInquiries",
                column: "RepairServiceId1",
                principalTable: "RepairServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceInquiries_RepairServices_RepairServiceId1",
                table: "ServiceInquiries");

            migrationBuilder.DropIndex(
                name: "IX_ServiceInquiries_RepairServiceId1",
                table: "ServiceInquiries");

            migrationBuilder.DropColumn(
                name: "RepairServiceId1",
                table: "ServiceInquiries");

            migrationBuilder.DropColumn(
                name: "EstimatedTimeHours",
                table: "RepairServices");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "RepairServices");

            migrationBuilder.DropColumn(
                name: "StartingPrice",
                table: "RepairServices");
        }
    }
}
