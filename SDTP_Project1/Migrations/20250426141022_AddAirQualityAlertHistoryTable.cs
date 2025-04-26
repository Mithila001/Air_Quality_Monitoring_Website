using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDTP_Project1.Migrations
{
    /// <inheritdoc />
    public partial class AddAirQualityAlertHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AirQualityAlertHistory",
                columns: table => new
                {
                    AlertHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SensorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MeasurementID = table.Column<int>(type: "int", nullable: false),
                    Parameter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentValue = table.Column<double>(type: "float", nullable: false),
                    ThresholdValue = table.Column<double>(type: "float", nullable: false),
                    AlertedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirQualityAlertHistory", x => x.AlertHistoryId);
                    table.ForeignKey(
                        name: "FK_AirQualityAlertHistory_AirQualityData_MeasurementID",
                        column: x => x.MeasurementID,
                        principalTable: "AirQualityData",
                        principalColumn: "MeasurementID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AirQualityAlertHistory_Sensors_SensorID",
                        column: x => x.SensorID,
                        principalTable: "Sensors",
                        principalColumn: "SensorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AirQualityAlertHistory_MeasurementID",
                table: "AirQualityAlertHistory",
                column: "MeasurementID");

            migrationBuilder.CreateIndex(
                name: "IX_AirQualityAlertHistory_SensorID",
                table: "AirQualityAlertHistory",
                column: "SensorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirQualityAlertHistory");
        }
    }
}
