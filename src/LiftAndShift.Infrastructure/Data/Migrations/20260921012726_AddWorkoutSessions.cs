using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LiftAndShift.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkoutSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkoutSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    FamilyMemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    Workout = table.Column<int>(type: "INTEGER", nullable: false),
                    TrainingPhase = table.Column<int>(type: "INTEGER", nullable: false),
                    PerformedOn = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoggedSets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Lift = table.Column<int>(type: "INTEGER", nullable: false),
                    WeightKg = table.Column<decimal>(type: "TEXT", nullable: false),
                    SetNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    RepsAchieved = table.Column<int>(type: "INTEGER", nullable: false),
                    WorkoutSessionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoggedSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoggedSets_WorkoutSessions_WorkoutSessionId",
                        column: x => x.WorkoutSessionId,
                        principalTable: "WorkoutSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoggedSets_WorkoutSessionId",
                table: "LoggedSets",
                column: "WorkoutSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LoggedSets");

            migrationBuilder.DropTable(
                name: "WorkoutSessions");
        }
    }
}
