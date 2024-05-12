using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booking.Data.Migrations
{
    /// <inheritdoc />
    public partial class user_table_housingChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Housings_UserId",
                table: "Housings");

            migrationBuilder.AddColumn<Guid>(
                name: "HousingId",
                table: "AspNetUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Housings_UserId",
                table: "Housings",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Housings_UserId",
                table: "Housings");

            migrationBuilder.DropColumn(
                name: "HousingId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Housings_UserId",
                table: "Housings",
                column: "UserId");
        }
    }
}
