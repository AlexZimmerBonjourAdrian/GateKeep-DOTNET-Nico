using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GateKeep.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddActivoToReglaAcceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "reglas_acceso",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Activo",
                table: "reglas_acceso");
        }
    }
}
