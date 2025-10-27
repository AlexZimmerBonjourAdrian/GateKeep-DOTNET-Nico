using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateKeep.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEspacioProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "espacios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Capacidad",
                table: "espacios",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CodigoEdificio",
                table: "espacios",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
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

            migrationBuilder.AddColumn<string>(
                name: "Nombre",
                table: "espacios",
                type: "text",
                nullable: false,
                defaultValue: "");

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
                name: "Ubicacion",
                table: "espacios",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "Capacidad",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "CodigoEdificio",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "EdificioId",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "EquipamientoEspecial",
                table: "espacios");

            migrationBuilder.DropColumn(
                name: "Nombre",
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
                name: "Ubicacion",
                table: "espacios");
        }
    }
}
