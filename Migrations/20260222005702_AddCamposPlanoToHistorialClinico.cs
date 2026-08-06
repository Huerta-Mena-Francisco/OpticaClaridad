using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpticaClaridad.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposPlanoToHistorialClinico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CilindroPlanoOD",
                table: "HistorialesClinicos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CilindroPlanoOI",
                table: "HistorialesClinicos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsPlanoOD",
                table: "HistorialesClinicos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EsPlanoOI",
                table: "HistorialesClinicos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CilindroPlanoOD",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "CilindroPlanoOI",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "EsPlanoOD",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "EsPlanoOI",
                table: "HistorialesClinicos");
        }
    }
}
