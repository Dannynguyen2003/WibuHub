using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WibuHub.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAtToStoryAuthorChapter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Stories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Chapters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Authors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ShippingAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "money", precision: 18, scale: 2, nullable: false),
                    Tax = table.Column<decimal>(type: "money", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "money", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderDetails",
                columns: table => new
                {
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "money", precision: 18, scale: 2, nullable: false),
                    Discount = table.Column<double>(type: "float", nullable: false),
                    Amount = table.Column<decimal>(type: "money", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetails", x => new { x.OrderId, x.ChapterId });
                    table.ForeignKey(
                        name: "FK_OrderDetails_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderDetails_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ChapterId",
                table: "OrderDetails",
                column: "ChapterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderDetails");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Authors");
        }
    }
}
