using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HairSalon.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexInBarberAvailabilityEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BarberAvailabilities_BarberId",
                table: "BarberAvailabilities");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "BarberAvailabilities",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "BarberAvailabilities",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BarberAvailabilities_BarberId_DayOfWeek",
                table: "BarberAvailabilities",
                columns: new[] { "BarberId", "DayOfWeek" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BarberAvailabilities_BarberId_DayOfWeek",
                table: "BarberAvailabilities");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BarberAvailabilities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BarberAvailabilities");

            migrationBuilder.CreateIndex(
                name: "IX_BarberAvailabilities_BarberId",
                table: "BarberAvailabilities",
                column: "BarberId");
        }
    }
}
