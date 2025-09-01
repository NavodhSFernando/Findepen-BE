using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinDepen_Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGoalEntityWithEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Goals",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Goals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedDate",
                table: "Goals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Goals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "LastUpdatedDate",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Goals");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Goals");
        }
    }
}
