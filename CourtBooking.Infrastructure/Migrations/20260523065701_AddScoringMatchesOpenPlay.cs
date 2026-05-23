using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourtBooking.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddScoringMatchesOpenPlay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoringMatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GameType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TargetScore = table.Column<int>(type: "int", nullable: false),
                    WinBy = table.Column<int>(type: "int", nullable: false),
                    TeamAScore = table.Column<int>(type: "int", nullable: false),
                    TeamBScore = table.Column<int>(type: "int", nullable: false),
                    ServingTeam = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ServerNumber = table.Column<int>(type: "int", nullable: true),
                    CurrentServerPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ScoreCall = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    WinnerTeam = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsOpenPlay = table.Column<bool>(type: "bit", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoringMatches_ScoreRuleSets_RuleSetId",
                        column: x => x.RuleSetId,
                        principalTable: "ScoreRuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScoringMatches_ScoreSports_SportId",
                        column: x => x.SportId,
                        principalTable: "ScoreSports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ScoringEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    RallyWinnerTeam = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    PreviousTeamAScore = table.Column<int>(type: "int", nullable: false),
                    PreviousTeamBScore = table.Column<int>(type: "int", nullable: false),
                    NewTeamAScore = table.Column<int>(type: "int", nullable: false),
                    NewTeamBScore = table.Column<int>(type: "int", nullable: false),
                    PreviousServingTeam = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NewServingTeam = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PreviousServerNumber = table.Column<int>(type: "int", nullable: true),
                    NewServerNumber = table.Column<int>(type: "int", nullable: true),
                    PreviousScoreCall = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewScoreCall = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoringEvents_ScoringMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "ScoringMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScoringTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TeamName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Score = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoringTeams_ScoringMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "ScoringMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScoringPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegisteredUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlayerOrder = table.Column<int>(type: "int", nullable: false),
                    IsGuest = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoringPlayers_ScoringMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "ScoringMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScoringPlayers_ScoringTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "ScoringTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoringEvents_MatchId",
                table: "ScoringEvents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringEvents_SequenceNumber",
                table: "ScoringEvents",
                column: "SequenceNumber");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringMatches_CreatedByUserId",
                table: "ScoringMatches",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringMatches_RuleSetId",
                table: "ScoringMatches",
                column: "RuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringMatches_SportId",
                table: "ScoringMatches",
                column: "SportId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringMatches_Status",
                table: "ScoringMatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringPlayers_MatchId",
                table: "ScoringPlayers",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringPlayers_TeamId",
                table: "ScoringPlayers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringTeams_MatchId",
                table: "ScoringTeams",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoringEvents");

            migrationBuilder.DropTable(
                name: "ScoringPlayers");

            migrationBuilder.DropTable(
                name: "ScoringTeams");

            migrationBuilder.DropTable(
                name: "ScoringMatches");
        }
    }
}
