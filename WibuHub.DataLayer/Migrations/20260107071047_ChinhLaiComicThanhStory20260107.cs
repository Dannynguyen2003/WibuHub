using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WibuHub.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class ChinhLaiComicThanhStory20260107 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnlockPrice",
                table: "Chapters");

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "Chapters",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Chapters",
                type: "money",
                precision: 9,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Chapters");

            migrationBuilder.AddColumn<decimal>(
                name: "UnlockPrice",
                table: "Chapters",
                type: "money",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
