using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class IP1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Email", "Username" },
                keyValues: new object[] { "omar@gmail.com", "Omar" },
                column: "IP",
                value: "1.1.1.1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumns: new[] { "Email", "Username" },
                keyValues: new object[] { "omar@gmail.com", "Omar" },
                column: "IP",
                value: null);
        }
    }
}
