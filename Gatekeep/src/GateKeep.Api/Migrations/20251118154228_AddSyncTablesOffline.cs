using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GateKeep.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSyncTablesOffline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DispositivosSync",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    UsuarioId = table.Column<long>(type: "bigint", nullable: false),
                    UltimaSincronizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UltimaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    Plataforma = table.Column<string>(type: "text", nullable: true),
                    VersionCliente = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispositivosSync", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventosOffline",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DeviceId = table.Column<string>(type: "text", nullable: false),
                    IdTemporal = table.Column<string>(type: "text", nullable: false),
                    TipoEvento = table.Column<string>(type: "text", nullable: false),
                    DatosEvento = table.Column<string>(type: "text", nullable: false),
                    FechaCreacionCliente = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaRecepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<string>(type: "text", nullable: false),
                    IntentosProcessamiento = table.Column<int>(type: "integer", nullable: false),
                    MensajeError = table.Column<string>(type: "text", nullable: true),
                    UltimaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdEventoPermanente = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosOffline", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispositivosSync");

            migrationBuilder.DropTable(
                name: "EventosOffline");
        }
    }
}
