using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinDepen_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetStartDateAndRenewalFrequency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RenewalFrequency",
                table: "Budgets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "Budgets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<double>(
                name: "BalanceAmount",
                table: "AspNetUsers",
                type: "float",
                nullable: true,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RenewalFrequency",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Budgets");

            migrationBuilder.AlterColumn<double>(
                name: "BalanceAmount",
                table: "AspNetUsers",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true,
                oldDefaultValue: 0.0);
        }
    }
}
