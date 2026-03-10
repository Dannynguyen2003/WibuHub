using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WibuHub.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addSlugforAuthor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Authors",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Authors");
        }
    }
}
