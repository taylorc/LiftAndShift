using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiftAndShift.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkWeightConsecutiveFailures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConsecutiveFailures",
                table: "WorkWeights",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsecutiveFailures",
                table: "WorkWeights");
        }
    }
}
