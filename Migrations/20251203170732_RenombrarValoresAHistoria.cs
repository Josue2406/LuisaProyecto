using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoLuisa.Migrations
{
    /// <inheritdoc />
    public partial class RenombrarValoresAHistoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Valores",
                table: "InformacionInstitucional",
                newName: "Historia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Historia",
                table: "InformacionInstitucional",
                newName: "Valores");
        }
    }
}
