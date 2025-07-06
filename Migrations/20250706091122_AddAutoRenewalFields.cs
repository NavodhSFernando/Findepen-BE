using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinDepen_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoRenewalFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRenewalEnabled",
                table: "Budgets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "Budgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRenewalDate",
                table: "Budgets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RenewalCount",
                table: "Budgets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRenewalEnabled",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "LastRenewalDate",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "RenewalCount",
                table: "Budgets");
        }
    }
}
