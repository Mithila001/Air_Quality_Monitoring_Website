using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDTP_Project1.Migrations
{
    /// <inheritdoc />
    public partial class FixAlertThresholdSettingPK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertThresholdSettings",
                columns: table => new
                {
                    ThresholdId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Parameter = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ThresholdValue = table.Column<float>(type: "real", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertThresholdSettings", x => x.ThresholdId);
                });

            migrationBuilder.CreateTable(
                name: "MonitoringAdmins",
                columns: table => new
                {
                    AdminId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringAdmins", x => x.AdminId);
                });

            migrationBuilder.CreateTable(
                name: "SimulationConfigurations",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FrequencyInSeconds = table.Column<int>(type: "int", nullable: false),
                    BaselinePM2_5 = table.Column<float>(type: "real", nullable: false),
                    BaselinePM10 = table.Column<float>(type: "real", nullable: false),
                    BaselineO3 = table.Column<float>(type: "real", nullable: false),
                    BaselineNO2 = table.Column<float>(type: "real", nullable: false),
                    BaselineSO2 = table.Column<float>(type: "real", nullable: false),
                    BaselineCO = table.Column<float>(type: "real", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationConfigurations", x => x.ConfigId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertThresholdSettings");

            migrationBuilder.DropTable(
                name: "MonitoringAdmins");

            migrationBuilder.DropTable(
                name: "SimulationConfigurations");
        }
    }
}
