using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiftAndShift.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkWeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkWeights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FamilyMemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    Lift = table.Column<int>(type: "INTEGER", nullable: false),
                    WeightKg = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkWeights", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkWeights_FamilyMemberId_Lift",
                table: "WorkWeights",
                columns: new[] { "FamilyMemberId", "Lift" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkWeights");
        }
    }
}
