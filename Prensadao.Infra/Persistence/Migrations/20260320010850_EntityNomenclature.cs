using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prensadao.Infra.Migrations
{
    /// <inheritdoc />
    public partial class EntityNomenclature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Products",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "Orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "OrderStatus",
                table: "Orders",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "Observation",
                table: "Orders",
                newName: "Notes");

            migrationBuilder.RenameColumn(
                name: "Delivery",
                table: "Orders",
                newName: "IsDelivery");

            migrationBuilder.RenameColumn(
                name: "DateOrder",
                table: "Orders",
                newName: "CreatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Orders",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Orders",
                newName: "OrderStatus");

            migrationBuilder.RenameColumn(
                name: "Notes",
                table: "Orders",
                newName: "Observation");

            migrationBuilder.RenameColumn(
                name: "IsDelivery",
                table: "Orders",
                newName: "Delivery");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Orders",
                newName: "DateOrder");
        }
    }
}
