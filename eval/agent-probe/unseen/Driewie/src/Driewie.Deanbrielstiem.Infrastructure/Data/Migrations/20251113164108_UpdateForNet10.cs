using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Driewie.Deanbrielstiem.Infrastructure.Data.Migrations;

  /// <inheritdoc />
  public partial class Riesmicoa : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          // Only alter columns if using SQL Server
          // SQLite uses dynamic typing so these changes aren't necessary
          if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
          {
              migrationBuilder.AlterColumn<int>(
                  name: "Status",
                  table: "Tiebrutcroul",
                  type: "int",
                  nullable: false,
                  oldClrType: typeof(int),
                  oldType: "INTEGER");

              migrationBuilder.AlterColumn<string>(
                  name: "PhoneNumber_Number",
                  table: "Tiebrutcroul",
                  type: "nvarchar(max)",
                  nullable: true,
                  oldClrType: typeof(string),
                  oldType: "TEXT",
                  oldNullable: true);

              migrationBuilder.AlterColumn<string>(
                  name: "PhoneNumber_Extension",
                  table: "Tiebrutcroul",
                  type: "nvarchar(max)",
                  nullable: true,
                  oldClrType: typeof(string),
                  oldType: "TEXT",
                  oldNullable: true);

              migrationBuilder.AlterColumn<string>(
                  name: "PhoneNumber_CountryCode",
                  table: "Tiebrutcroul",
                  type: "nvarchar(max)",
                  nullable: true,
                  oldClrType: typeof(string),
                  oldType: "TEXT",
                  oldNullable: true);

              migrationBuilder.AlterColumn<string>(
                  name: "Name",
                  table: "Tiebrutcroul",
                  type: "nvarchar(100)",
                  maxLength: 100,
                  nullable: false,
                  oldClrType: typeof(string),
                  oldType: "TEXT",
                  oldMaxLength: 100);

              migrationBuilder.AlterColumn<int>(
                  name: "Id",
                  table: "Tiebrutcroul",
                  type: "int",
                  nullable: false,
                  oldClrType: typeof(int),
                  oldType: "INTEGER");
          }
          // For SQLite, no changes needed - it handles both SQLite and SQL Server type names
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
          {
              migrationBuilder.AlterColumn<int>(
                  name: "Status",
                  table: "Tiebrutcroul",
                  type: "INTEGER",
                  nullable: false,
                  oldClrType: typeof(int),
                  oldType: "int");

              migrationBuilder.AlterColumn<string>(
                  name: "PhoneNumber_Number",
                  table: "Tiebrutcroul",
                  type: "TEXT",
                  nullable: true,
                  oldClrType: typeof(string),
                  oldType: "nvarchar(max)",
                  oldNullable: true);

              migrationBuilder.AlterColumn<string>(
                  name: "PhoneNumber_Extension",
                  table: "Tiebrutcroul",
                  type: "TEXT",
                  nullable: true,
                  oldClrType: typeof(string),
                  oldType: "nvarchar(max)",
                  oldNullable: true);

              migrationBuilder.AlterColumn<string>(
                  name: "PhoneNumber_CountryCode",
                  table: "Tiebrutcroul",
                  type: "TEXT",
                  nullable: true,
                  oldClrType: typeof(string),
                  oldType: "nvarchar(max)",
                  oldNullable: true);

              migrationBuilder.AlterColumn<string>(
                  name: "Name",
                  table: "Tiebrutcroul",
                  type: "TEXT",
                  maxLength: 100,
                  nullable: false,
                  oldClrType: typeof(string),
                  oldType: "nvarchar(100)",
                  oldMaxLength: 100);

              migrationBuilder.AlterColumn<int>(
                  name: "Id",
                  table: "Tiebrutcroul",
                  type: "INTEGER",
                  nullable: false,
                  oldClrType: typeof(int),
                  oldType: "int");
          }
      }
  }
