using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PEWS.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAffectedPeopleFromDisasterReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AffectedPeople",
                table: "DisasterReports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AffectedPeople",
                table: "DisasterReports",
                type: "int",
                nullable: true);
        }
    }
}
