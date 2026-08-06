using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OpticaClaridad.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasProducto", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteNombre = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TipoCita = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    RecordatorioEnviado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Eliminada = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: true, defaultValueSql: "CURRENT_DATE"),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Configuraciones",
                columns: table => new
                {
                    Clave = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Valor = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuraciones", x => x.Clave);
                });

            migrationBuilder.CreateTable(
                name: "Pacientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: true, defaultValueSql: "CURRENT_DATE"),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Sexo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pacientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Contacto = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Notas = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreCompleto = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NombreUsuario = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Clave = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PrecioCompra = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    StockMinimo = table.Column<int>(type: "INTEGER", nullable: false),
                    ImagenPrincipalUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Marca = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Modelo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Material = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Genero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TipoLente = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TieneUV = table.Column<bool>(type: "INTEGER", nullable: false),
                    TieneAntireflejante = table.Column<bool>(type: "INTEGER", nullable: false),
                    EsFotocromatico = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_CategoriasProducto_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "CategoriasProducto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamenesVisual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaExamen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ODEsferico = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ODCilindrico = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ODEje = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ODAdicion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OIEsferico = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OICilindrico = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OIEje = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OIAdicion = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DistanciaPupilar = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamenesVisual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamenesVisual_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialesClinicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaExamen = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraExamen = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    MotivoExamen = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    TipoExamen = table.Column<string>(type: "TEXT", nullable: false),
                    CostoExamen = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    ExamenPagado = table.Column<bool>(type: "INTEGER", nullable: false),
                    TieneDiabetes = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiabetesTratamiento = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TieneHipertension = table.Column<bool>(type: "INTEGER", nullable: false),
                    HipertensionTratamiento = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TieneAlergias = table.Column<bool>(type: "INTEGER", nullable: false),
                    OtrasPatologias = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UsaLentes = table.Column<bool>(type: "INTEGER", nullable: false),
                    TipoLentesActuales = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AntecedentesOculares = table.Column<bool>(type: "INTEGER", nullable: false),
                    DetalleAntecedentes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EsferaOD = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CilindroOD = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EjeOD = table.Column<int>(type: "INTEGER", nullable: true),
                    EsferaOI = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CilindroOI = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    EjeOI = table.Column<int>(type: "INTEGER", nullable: true),
                    Adicion = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DistanciaPupilar = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    AgudezaVisualODLejos = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    AgudezaVisualOILejos = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    AgudezaVisualODCerca = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    AgudezaVisualOICerca = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    ObservacionesExamen = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Recomendaciones = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    NecesitaSeguimiento = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaProximoControl = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InteresEnCompra = table.Column<bool>(type: "INTEGER", nullable: false),
                    ObservacionesInteres = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Optometrista = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialesClinicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialesClinicos_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NumeroVenta = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    PacienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaVenta = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TipoPago = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cambio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<string>(type: "TEXT", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ventas_Pacientes_PacienteId",
                        column: x => x.PacienteId,
                        principalTable: "Pacientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ProductosImagenes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImagenUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    EsPrincipal = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductosImagenes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductosImagenes_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CancelacionesVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaCancelacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancelacionesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancelacionesVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Creditos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Saldo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaCredito = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DiasCredito = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoPeriodo = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    CantidadPeriodo = table.Column<int>(type: "INTEGER", nullable: false),
                    MontoPeriodo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creditos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Creditos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Creditos_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetallesVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Cantidad = table.Column<int>(type: "INTEGER", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetallesVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AbonosCredito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CreditoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaAbono = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UsuarioId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AbonosCredito", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AbonosCredito_Creditos_CreditoId",
                        column: x => x.CreditoId,
                        principalTable: "Creditos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AbonosCredito_CreditoId",
                table: "AbonosCredito",
                column: "CreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_CancelacionesVenta_VentaId",
                table: "CancelacionesVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_ClienteId",
                table: "Creditos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_VentaId",
                table: "Creditos",
                column: "VentaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_ProductoId",
                table: "DetallesVenta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVenta_VentaId",
                table: "DetallesVenta",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamenesVisual_PacienteId",
                table: "ExamenesVisual",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialesClinicos_PacienteId",
                table: "HistorialesClinicos",
                column: "PacienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductosImagenes_ProductoId",
                table: "ProductosImagenes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_PacienteId",
                table: "Ventas",
                column: "PacienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AbonosCredito");

            migrationBuilder.DropTable(
                name: "CancelacionesVenta");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "Configuraciones");

            migrationBuilder.DropTable(
                name: "DetallesVenta");

            migrationBuilder.DropTable(
                name: "ExamenesVisual");

            migrationBuilder.DropTable(
                name: "HistorialesClinicos");

            migrationBuilder.DropTable(
                name: "ProductosImagenes");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Creditos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "CategoriasProducto");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Pacientes");
        }
    }
}
