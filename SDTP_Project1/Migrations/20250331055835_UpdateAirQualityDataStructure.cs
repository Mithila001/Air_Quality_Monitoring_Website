using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDTP_Project1.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAirQualityDataStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sensors",
                columns: table => new
                {
                    SensorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensors", x => x.SensorID);
                });

            migrationBuilder.CreateTable(
                name: "AirQualityData",
                columns: table => new
                {
                    MeasurementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SensorID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PM2_5 = table.Column<double>(type: "float", nullable: true),
                    PM10 = table.Column<double>(type: "float", nullable: true),
                    O3 = table.Column<double>(type: "float", nullable: true),
                    NO2 = table.Column<double>(type: "float", nullable: true),
                    SO2 = table.Column<double>(type: "float", nullable: true),
                    CO = table.Column<double>(type: "float", nullable: true),
                    AQI = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AirQualityData", x => x.MeasurementID);
                    table.ForeignKey(
                        name: "FK_AirQualityData_Sensors_SensorID",
                        column: x => x.SensorID,
                        principalTable: "Sensors",
                        principalColumn: "SensorID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AirQualityData_SensorID",
                table: "AirQualityData",
                column: "SensorID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AirQualityData");

            migrationBuilder.DropTable(
                name: "Sensors");
        }
    }
}
