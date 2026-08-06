using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpticaClaridad.Migrations
{
    /// <inheritdoc />
    public partial class AgregarGraduacionAnterior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GraduacionAnterior",
                table: "HistorialesClinicos",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ODAddAnterior",
                table: "HistorialesClinicos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ODCilAnterior",
                table: "HistorialesClinicos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ODEjeAnterior",
                table: "HistorialesClinicos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ODEsfAnterior",
                table: "HistorialesClinicos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OIAddAnterior",
                table: "HistorialesClinicos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OICilAnterior",
                table: "HistorialesClinicos",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OIEjeAnterior",
                table: "HistorialesClinicos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OIEsfAnterior",
                table: "HistorialesClinicos",
                type: "decimal(5,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GraduacionAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "ODAddAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "ODCilAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "ODEjeAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "ODEsfAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "OIAddAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "OICilAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "OIEjeAnterior",
                table: "HistorialesClinicos");

            migrationBuilder.DropColumn(
                name: "OIEsfAnterior",
                table: "HistorialesClinicos");
        }
    }
}
