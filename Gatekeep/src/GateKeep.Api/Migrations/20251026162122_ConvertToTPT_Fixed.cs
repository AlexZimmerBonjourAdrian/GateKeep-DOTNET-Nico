using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateKeep.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToTPT_Fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoEdificio",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "EdificioId",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "EquipamientoEspecial",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "NumeroLaboratorio",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "NumeroPisos",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "NumeroSalon",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "Salon_EdificioId",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "TipoLaboratorio",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "TipoSalon",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "tipo",
                table: "espacios");

            migrationBuilder.AlterColumn<string>(
                name: "Ubicacion",
                table: "espacios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "espacios",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "espacios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "espacios",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

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
                name: "IX_espacios_activo",
                table: "espacios",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_espacios_nombre",
                table: "espacios",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_edificios_codigo",
                table: "edificios",
                column: "CodigoEdificio",
                unique: true,
                filter: "\"CodigoEdificio\" IS NOT NULL");

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
                name: "IX_salones_edificio_id",
                table: "salones",
                column: "EdificioId");

            migrationBuilder.CreateIndex(
                name: "IX_salones_edificio_numero",
                table: "salones",
                columns: new[] { "EdificioId", "NumeroSalon" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "laboratorios");

            migrationBuilder.DropTable(
                name: "salones");

            migrationBuilder.DropTable(
                name: "edificios");

            migrationBuilder.DropIndex(
                name: "IX_espacios_activo",
                table: "espacios");

            migrationBuilder.DropIndex(
                name: "IX_espacios_nombre",
                table: "espacios");

            migrationBuilder.AlterColumn<string>(
                name: "Ubicacion",
                table: "espacios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "espacios",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "espacios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "Activo",
                table: "espacios",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "CodigoEdificio",
                table: "espacios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EdificioId",
                table: "espacios",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EquipamientoEspecial",
                table: "espacios",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroLaboratorio",
                table: "espacios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroPisos",
                table: "espacios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroSalon",
                table: "espacios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Salon_EdificioId",
                table: "espacios",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoLaboratorio",
                table: "espacios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoSalon",
                table: "espacios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo",
                table: "espacios",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");
        }
    }
}
