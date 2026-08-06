using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpticaClaridad.Migrations
{
    /// <inheritdoc />
    public partial class EliminarClientesYActualizarAPacientes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creditos_Clientes_ClienteId",
                table: "Creditos");

            migrationBuilder.DropForeignKey(
                name: "FK_Creditos_Ventas_VentaId",
                table: "Creditos");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Ventas");

            migrationBuilder.RenameColumn(
                name: "ClienteId",
                table: "Creditos",
                newName: "PacienteId");

            migrationBuilder.RenameIndex(
                name: "IX_Creditos_ClienteId",
                table: "Creditos",
                newName: "IX_Creditos_PacienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Creditos_Pacientes_PacienteId",
                table: "Creditos",
                column: "PacienteId",
                principalTable: "Pacientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Creditos_Ventas_VentaId",
                table: "Creditos",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Creditos_Pacientes_PacienteId",
                table: "Creditos");

            migrationBuilder.DropForeignKey(
                name: "FK_Creditos_Ventas_VentaId",
                table: "Creditos");

            migrationBuilder.RenameColumn(
                name: "PacienteId",
                table: "Creditos",
                newName: "ClienteId");

            migrationBuilder.RenameIndex(
                name: "IX_Creditos_PacienteId",
                table: "Creditos",
                newName: "IX_Creditos_ClienteId");

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Ventas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: true, defaultValueSql: "CURRENT_DATE"),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Creditos_Clientes_ClienteId",
                table: "Creditos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Creditos_Ventas_VentaId",
                table: "Creditos",
                column: "VentaId",
                principalTable: "Ventas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Clientes_ClienteId",
                table: "Ventas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
