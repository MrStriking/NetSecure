using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class SelectedLevel1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Email", "Username" },
                keyValues: new object[] { "omar@gmail.com", "Omar" },
                column: "SelectedLevel",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Email", "Username" },
                keyValues: new object[] { "omar@gmail.com", "Omar" },
                column: "SelectedLevel",
                value: "");
        }
    }
}
