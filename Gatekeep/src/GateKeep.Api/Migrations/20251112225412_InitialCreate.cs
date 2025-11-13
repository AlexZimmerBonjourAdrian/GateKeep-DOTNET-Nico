using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GateKeep.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "anuncios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anuncios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "beneficios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Vigencia = table.Column<bool>(type: "boolean", nullable: false),
                    FechaDeVencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Cupos = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_beneficios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "espacios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Ubicacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Capacidad = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_espacios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eventos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Resultado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PuntoControl = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Apellido = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Contrasenia = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    FechaAlta = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Credencial = table.Column<int>(type: "integer", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "edificios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    NumeroPisos = table.Column<int>(type: "integer", nullable: false),
                    CodigoEdificio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_edificios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_edificios_espacios_Id",
                        column: x => x.Id,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reglas_acceso",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    HorarioApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HorarioCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VigenciaApertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VigenciaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RolesPermitidos = table.Column<string>(type: "text", nullable: false),
                    EspacioId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reglas_acceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reglas_acceso_espacios_EspacioId",
                        column: x => x.EspacioId,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "beneficios_usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    BeneficioId = table.Column<long>(type: "bigint", nullable: false),
                    EstadoCanje = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_beneficios_usuarios", x => new { x.UsuarioId, x.BeneficioId });
                    table.ForeignKey(
                        name: "FK_beneficios_usuarios_beneficios_BeneficioId",
                        column: x => x.BeneficioId,
                        principalTable: "beneficios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_beneficios_usuarios_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eventos_acceso",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Resultado = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PuntoControl = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    EspacioId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_acceso", x => x.Id);
                    table.ForeignKey(
                        name: "FK_eventos_acceso_espacios_EspacioId",
                        column: x => x.EspacioId,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventos_acceso_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_espacios",
                columns: table => new
                {
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    EspacioId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_espacios", x => new { x.UsuarioId, x.EspacioId });
                    table.ForeignKey(
                        name: "FK_usuarios_espacios_espacios_EspacioId",
                        column: x => x.EspacioId,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_espacios_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "laboratorios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    EdificioId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroLaboratorio = table.Column<int>(type: "integer", nullable: false),
                    TipoLaboratorio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EquipamientoEspecial = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_laboratorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_laboratorios_edificios_EdificioId",
                        column: x => x.EdificioId,
                        principalTable: "edificios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_laboratorios_espacios_Id",
                        column: x => x.Id,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "salones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    EdificioId = table.Column<long>(type: "bigint", nullable: false),
                    NumeroSalon = table.Column<int>(type: "integer", nullable: false),
                    TipoSalon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_salones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_salones_edificios_EdificioId",
                        column: x => x.EdificioId,
                        principalTable: "edificios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_salones_espacios_Id",
                        column: x => x.Id,
                        principalTable: "espacios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_anuncios_fecha",
                table: "anuncios",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_beneficios_usuarios_beneficio_id",
                table: "beneficios_usuarios",
                column: "BeneficioId");

            migrationBuilder.CreateIndex(
                name: "IX_beneficios_usuarios_usuario_id",
                table: "beneficios_usuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_edificios_codigo",
                table: "edificios",
                column: "CodigoEdificio",
                unique: true,
                filter: "\"CodigoEdificio\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_espacios_activo",
                table: "espacios",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_espacios_nombre",
                table: "espacios",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_fecha",
                table: "eventos",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_acceso_espacio_id",
                table: "eventos_acceso",
                column: "EspacioId");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_acceso_fecha",
                table: "eventos_acceso",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_acceso_usuario_id",
                table: "eventos_acceso",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_laboratorios_edificio_id",
                table: "laboratorios",
                column: "EdificioId");

            migrationBuilder.CreateIndex(
                name: "IX_laboratorios_edificio_numero",
                table: "laboratorios",
                columns: new[] { "EdificioId", "NumeroLaboratorio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reglas_acceso_espacio_id",
                table: "reglas_acceso",
                column: "EspacioId");

            migrationBuilder.CreateIndex(
                name: "IX_salones_edificio_id",
                table: "salones",
                column: "EdificioId");

            migrationBuilder.CreateIndex(
                name: "IX_salones_edificio_numero",
                table: "salones",
                columns: new[] { "EdificioId", "NumeroSalon" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_credencial",
                table: "usuarios",
                column: "Credencial");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_rol",
                table: "usuarios",
                column: "Rol");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_espacios_espacio_id",
                table: "usuarios_espacios",
                column: "EspacioId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_espacios_usuario_id",
                table: "usuarios_espacios",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anuncios");

            migrationBuilder.DropTable(
                name: "beneficios_usuarios");

            migrationBuilder.DropTable(
                name: "eventos");

            migrationBuilder.DropTable(
                name: "eventos_acceso");

            migrationBuilder.DropTable(
                name: "laboratorios");

            migrationBuilder.DropTable(
                name: "reglas_acceso");

            migrationBuilder.DropTable(
                name: "salones");

            migrationBuilder.DropTable(
                name: "usuarios_espacios");

            migrationBuilder.DropTable(
                name: "beneficios");

            migrationBuilder.DropTable(
                name: "edificios");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "espacios");
        }
    }
}
