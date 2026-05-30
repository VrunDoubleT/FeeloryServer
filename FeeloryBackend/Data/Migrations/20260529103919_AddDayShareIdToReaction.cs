using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeeloryBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDayShareIdToReaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
   
            migrationBuilder.AddColumn<Guid>(
                name: "DayShareId",
                table: "Reactions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PostedAt",
                table: "DayShareFeeds",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_DayShareId",
                table: "Reactions",
                column: "DayShareId");

            migrationBuilder.AddForeignKey(
                name: "FK_DayShareFeeds_DayShares_DayShareId",
                table: "DayShareFeeds",
                column: "DayShareId",
                principalTable: "DayShares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DayShareFeeds_Users_ViewerId",
                table: "DayShareFeeds",
                column: "ViewerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_DayShares_DayShareId",
                table: "Reactions",
                column: "DayShareId",
                principalTable: "DayShares",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DayShareFeeds_DayShares_DayShareId",
                table: "DayShareFeeds");

            migrationBuilder.DropForeignKey(
                name: "FK_DayShareFeeds_Users_ViewerId",
                table: "DayShareFeeds");

            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_DayShares_DayShareId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_DayShareId",
                table: "Reactions");

            migrationBuilder.DropColumn(
                name: "DayShareId",
                table: "Reactions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PostedAt",
                table: "DayShareFeeds",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddForeignKey(
                name: "FK_DayShareFeeds_DayShares_DayShareId",
                table: "DayShareFeeds",
                column: "DayShareId",
                principalTable: "DayShares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DayShareFeeds_Users_ViewerId",
                table: "DayShareFeeds",
                column: "ViewerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
