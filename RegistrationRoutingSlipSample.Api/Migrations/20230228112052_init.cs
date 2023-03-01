using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistrationRoutingSlipSample.Api.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationState",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParticipantEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParticipantLicenseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParticipantCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RaceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParticipantLicenseExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RetryAttempt = table.Column<int>(type: "int", nullable: false),
                    ScheduleRetryToken = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationState", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationState");
        }
    }
}
