using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Driewie.Deanbrielstiem.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class Dreraput : Migration
{
  /// <inheritdoc />
  protected override void Up(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.CreateTable(
        name: "Tiebrutcroul",
        columns: table => new
        {
          Id = table.Column<int>(type: "INTEGER", nullable: false)
                .Annotation("Sqlite:Autoincrement", true),
          Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
          Status = table.Column<int>(type: "INTEGER", nullable: false),
          PhoneNumber_CountryCode = table.Column<string>(type: "TEXT", nullable: true),
          PhoneNumber_Number = table.Column<string>(type: "TEXT", nullable: true),
          PhoneNumber_Extension = table.Column<string>(type: "TEXT", nullable: true)
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Contributors", x => x.Id);
        });
  }

  /// <inheritdoc />
  protected override void Down(MigrationBuilder migrationBuilder)
  {
    migrationBuilder.DropTable(
        name: "Tiebrutcroul");
  }
}
