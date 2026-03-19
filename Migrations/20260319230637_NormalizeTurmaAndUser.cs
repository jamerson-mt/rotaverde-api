using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RotaVerdeAPI.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeTurmaAndUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Turmas_TurmaModelId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TurmaModelId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TurmaModelId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "TurmaId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TurmaId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TurmaModelId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TurmaModelId",
                table: "AspNetUsers",
                column: "TurmaModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Turmas_TurmaModelId",
                table: "AspNetUsers",
                column: "TurmaModelId",
                principalTable: "Turmas",
                principalColumn: "Id");
        }
    }
}
