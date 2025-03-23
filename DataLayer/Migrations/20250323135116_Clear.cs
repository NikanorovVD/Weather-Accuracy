using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class Clear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtmosphericPressureMax",
                table: "WeaterRecords");

            migrationBuilder.DropColumn(
                name: "CloudCover",
                table: "WeaterRecords");

            migrationBuilder.DropColumn(
                name: "GeomagneticActivity",
                table: "WeaterRecords");

            migrationBuilder.DropColumn(
                name: "UVIndex",
                table: "WeaterRecords");

            migrationBuilder.DropColumn(
                name: "WindDirection",
                table: "WeaterRecords");

            migrationBuilder.RenameColumn(
                name: "AtmosphericPressureMin",
                table: "WeaterRecords",
                newName: "AtmosphericPressureAvg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AtmosphericPressureAvg",
                table: "WeaterRecords",
                newName: "AtmosphericPressureMin");

            migrationBuilder.AddColumn<decimal>(
                name: "AtmosphericPressureMax",
                table: "WeaterRecords",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CloudCover",
                table: "WeaterRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "GeomagneticActivity",
                table: "WeaterRecords",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "UVIndex",
                table: "WeaterRecords",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WindDirection",
                table: "WeaterRecords",
                type: "text",
                nullable: true);
        }
    }
}
