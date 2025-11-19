using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoLuisa.Migrations
{
    /// <inheritdoc />
    public partial class ActualizarModeloHorario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Grupo",
                table: "Horarios");

            migrationBuilder.DropColumn(
                name: "Profesor",
                table: "Horarios");

            migrationBuilder.UpdateData(
                table: "Horarios",
                keyColumn: "Seccion",
                keyValue: null,
                column: "Seccion",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Seccion",
                table: "Horarios",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DocenteId",
                table: "Horarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocenteId",
                table: "Horarios");

            migrationBuilder.AlterColumn<string>(
                name: "Seccion",
                table: "Horarios",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(30)",
                oldMaxLength: 30)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Grupo",
                table: "Horarios",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Profesor",
                table: "Horarios",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
