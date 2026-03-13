using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WibuHub.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddStoryPriceDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Chapters_ChapterId",
                table: "OrderDetails");

            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Discount' AND Object_ID = Object_ID(N'[Chapters]'))
    ALTER TABLE [Chapters] DROP COLUMN [Discount];
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Price' AND Object_ID = Object_ID(N'[Chapters]'))
    ALTER TABLE [Chapters] DROP COLUMN [Price];
");

            migrationBuilder.RenameColumn(
                name: "ChapterId",
                table: "OrderDetails",
                newName: "StoryId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_ChapterId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_StoryId");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Discount' AND Object_ID = Object_ID(N'[Stories]'))
    ALTER TABLE [Stories] ADD [Discount] decimal(5,2) NOT NULL CONSTRAINT [DF_Stories_Discount] DEFAULT (0);
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Price' AND Object_ID = Object_ID(N'[Stories]'))
    ALTER TABLE [Stories] ADD [Price] money NOT NULL CONSTRAINT [DF_Stories_Price] DEFAULT (0);
");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Stories_StoryId",
                table: "OrderDetails",
                column: "StoryId",
                principalTable: "Stories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Stories_StoryId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Stories");

            migrationBuilder.RenameColumn(
                name: "StoryId",
                table: "OrderDetails",
                newName: "ChapterId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderDetails_StoryId",
                table: "OrderDetails",
                newName: "IX_OrderDetails_ChapterId");

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
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Chapters_ChapterId",
                table: "OrderDetails",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
