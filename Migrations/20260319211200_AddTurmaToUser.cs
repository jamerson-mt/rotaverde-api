using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotaVerdeAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTurmaToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TurmaId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TurmaId",
                table: "AspNetUsers");
        }
    }
}
