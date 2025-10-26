using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GateKeep.Api.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                });

            migrationBuilder.CreateTable(
                name: "espacios",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_espacios", x => x.Id);
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
                });

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Mensaje = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notificaciones_usuarios",
                columns: table => new
                {
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    NotificacionId = table.Column<long>(type: "bigint", nullable: false),
                    Leido = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones_usuarios", x => new { x.UsuarioId, x.NotificacionId });
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
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Credencial = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "beneficios");

            migrationBuilder.DropTable(
                name: "beneficios_usuarios");

            migrationBuilder.DropTable(
                name: "espacios");

            migrationBuilder.DropTable(
                name: "eventos_acceso");

            migrationBuilder.DropTable(
                name: "notificaciones");

            migrationBuilder.DropTable(
                name: "notificaciones_usuarios");

            migrationBuilder.DropTable(
                name: "reglas_acceso");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "usuarios_espacios");
        }
    }
}
