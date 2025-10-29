using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoLuisa.Migrations
{
    /// <inheritdoc />
    public partial class AddSeccionToHorarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Seccion",
                table: "Horarios",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seccion",
                table: "Horarios");
        }
    }
}
