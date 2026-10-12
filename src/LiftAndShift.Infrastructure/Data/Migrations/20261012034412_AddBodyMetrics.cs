using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiftAndShift.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBodyMetrics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FamilyMemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    BodyWeightKg = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyMetrics", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BodyMetrics");
        }
    }
}
