using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class Region : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "WeaterRecords",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "DateTime",
                table: "WeaterRecords",
                newName: "MadeOnDateTime");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "WeaterRecords",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Region",
                table: "WeaterRecords");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "WeaterRecords",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "MadeOnDateTime",
                table: "WeaterRecords",
                newName: "DateTime");
        }
    }
}
