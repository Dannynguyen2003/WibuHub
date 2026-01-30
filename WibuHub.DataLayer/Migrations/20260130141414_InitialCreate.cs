using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WibuHub.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryGenres");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Genres",
                table: "Genres");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Genres");

            migrationBuilder.RenameTable(
                name: "Genres",
                newName: "StoryCategories");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Stories",
                newName: "StoryName");

            migrationBuilder.RenameColumn(
                name: "Thumbnail",
                table: "Stories",
                newName: "CoverImage");

            migrationBuilder.RenameColumn(
                name: "DateCreated",
                table: "Stories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Chapters",
                newName: "ChapterNumber");

            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "Chapters",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Chapters_StoryId_Number",
                table: "Chapters",
                newName: "IX_Chapters_StoryId_ChapterNumber");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "StoryCategories",
                newName: "CategoryId");

            migrationBuilder.AddColumn<string>(
                name: "AuthorName",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StoryName",
                table: "Chapters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(75)",
                maxLength: 75,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "StoryId",
                table: "StoryCategories",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_StoryCategories",
                table: "StoryCategories",
                columns: new[] { "StoryId", "CategoryId" });

            migrationBuilder.CreateTable(
                name: "ChapterImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    StorageType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChapterImages_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StoryCategories_CategoryId",
                table: "StoryCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterImages_ChapterId",
                table: "ChapterImages",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoryCategories_Categories_CategoryId",
                table: "StoryCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StoryCategories_Stories_StoryId",
                table: "StoryCategories",
                column: "StoryId",
                principalTable: "Stories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoryCategories_Categories_CategoryId",
                table: "StoryCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_StoryCategories_Stories_StoryId",
                table: "StoryCategories");

            migrationBuilder.DropTable(
                name: "ChapterImages");

            migrationBuilder.DropIndex(
                name: "IX_Category_Slug",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StoryCategories",
                table: "StoryCategories");

            migrationBuilder.DropIndex(
                name: "IX_StoryCategories_CategoryId",
                table: "StoryCategories");

            migrationBuilder.DropColumn(
                name: "AuthorName",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "StoryName",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "StoryId",
                table: "StoryCategories");

            migrationBuilder.RenameTable(
                name: "StoryCategories",
                newName: "Genres");

            migrationBuilder.RenameColumn(
                name: "StoryName",
                table: "Stories",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Stories",
                newName: "DateCreated");

            migrationBuilder.RenameColumn(
                name: "CoverImage",
                table: "Stories",
                newName: "Thumbnail");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Chapters",
                newName: "CreateDate");

            migrationBuilder.RenameColumn(
                name: "ChapterNumber",
                table: "Chapters",
                newName: "Number");

            migrationBuilder.RenameIndex(
                name: "IX_Chapters_StoryId_ChapterNumber",
                table: "Chapters",
                newName: "IX_Chapters_StoryId_Number");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Genres",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Genres",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Genres",
                table: "Genres",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "StoryGenres",
                columns: table => new
                {
                    GenresId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoriesId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryGenres", x => new { x.GenresId, x.StoriesId });
                    table.ForeignKey(
                        name: "FK_StoryGenres_Genres_GenresId",
                        column: x => x.GenresId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoryGenres_Stories_StoriesId",
                        column: x => x.StoriesId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryGenres_StoriesId",
                table: "StoryGenres",
                column: "StoriesId");
        }
    }
}
