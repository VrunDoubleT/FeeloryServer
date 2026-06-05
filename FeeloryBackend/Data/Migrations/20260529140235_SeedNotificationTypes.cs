using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeeloryBackend.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedNotificationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_DayShares_DayShareId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_DayShareId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_PostId_UserId",
                table: "Reactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Reactions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_DayShareId_UserId",
                table: "Reactions",
                columns: new[] { "DayShareId", "UserId" },
                unique: true,
                filter: "[DayShareId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_PostId_UserId",
                table: "Reactions",
                columns: new[] { "PostId", "UserId" },
                unique: true,
                filter: "[PostId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_DayShares_DayShareId",
                table: "Reactions",
                column: "DayShareId",
                principalTable: "DayShares",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reactions_DayShares_DayShareId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_DayShareId_UserId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_PostId_UserId",
                table: "Reactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "PostId",
                table: "Reactions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_DayShareId",
                table: "Reactions",
                column: "DayShareId");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_PostId_UserId",
                table: "Reactions",
                columns: new[] { "PostId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reactions_DayShares_DayShareId",
                table: "Reactions",
                column: "DayShareId",
                principalTable: "DayShares",
                principalColumn: "Id");
        }
    }
}
