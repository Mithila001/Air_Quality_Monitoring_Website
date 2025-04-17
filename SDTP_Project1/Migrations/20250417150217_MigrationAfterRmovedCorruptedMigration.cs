using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SDTP_Project1.Migrations
{
    /// <inheritdoc />
    public partial class MigrationAfterRmovedCorruptedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AirQualityData_Sensors_SensorID",
                table: "AirQualityData");

            migrationBuilder.AddForeignKey(
                name: "FK_AirQualityData_Sensors_SensorID",
                table: "AirQualityData",
                column: "SensorID",
                principalTable: "Sensors",
                principalColumn: "SensorID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AirQualityData_Sensors_SensorID",
                table: "AirQualityData");

            migrationBuilder.AddForeignKey(
                name: "FK_AirQualityData_Sensors_SensorID",
                table: "AirQualityData",
                column: "SensorID",
                principalTable: "Sensors",
                principalColumn: "SensorID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
