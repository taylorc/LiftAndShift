using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiftAndShift.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProgrammeSessionConsecutiveFailures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsecutiveFailures",
                table: "ProgrammeSessions",
                type: "jsonb",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsecutiveFailures",
                table: "ProgrammeSessions");
        }
    }
}
