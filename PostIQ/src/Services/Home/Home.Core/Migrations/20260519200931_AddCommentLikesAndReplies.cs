using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Home.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentLikesAndReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                schema: "Home",
                table: "PostComments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "ParentCommentId",
                schema: "Home",
                table: "PostComments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CommentLikes",
                schema: "Home",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommentLikes_PostComments_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "Home",
                        principalTable: "PostComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PostComments_ParentCommentId",
                schema: "Home",
                table: "PostComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_CommentId",
                schema: "Home",
                table: "CommentLikes",
                column: "CommentId");

            migrationBuilder.AddForeignKey(
                name: "FK_PostComments_PostComments_ParentCommentId",
                schema: "Home",
                table: "PostComments",
                column: "ParentCommentId",
                principalSchema: "Home",
                principalTable: "PostComments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PostComments_PostComments_ParentCommentId",
                schema: "Home",
                table: "PostComments");

            migrationBuilder.DropTable(
                name: "CommentLikes",
                schema: "Home");

            migrationBuilder.DropIndex(
                name: "IX_PostComments_ParentCommentId",
                schema: "Home",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "LikeCount",
                schema: "Home",
                table: "PostComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                schema: "Home",
                table: "PostComments");
        }
    }
}
