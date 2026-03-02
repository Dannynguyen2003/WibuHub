using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WibuHub.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addfunctionaddimage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChapterImages_ChapterId",
                table: "ChapterImages");

            migrationBuilder.AlterColumn<int>(
                name: "StorageType",
                table: "ChapterImages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterImages_ChapterId_OrderIndex",
                table: "ChapterImages",
                columns: new[] { "ChapterId", "OrderIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChapterImages_ChapterId_OrderIndex",
                table: "ChapterImages");

            migrationBuilder.AlterColumn<int>(
                name: "StorageType",
                table: "ChapterImages",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldDefaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterImages_ChapterId",
                table: "ChapterImages",
                column: "ChapterId");
        }
    }
}
