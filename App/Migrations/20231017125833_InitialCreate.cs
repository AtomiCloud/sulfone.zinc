using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class InitialCreate : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "Users",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Sub = table.Column<string>(type: "text", nullable: false),
            Username = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Users", x => x.Id);
          });

      migrationBuilder.CreateIndex(
          name: "IX_Users_Sub",
          table: "Users",
          column: "Sub",
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_Users_Username",
          table: "Users",
          column: "Username",
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "Users");
    }
  }
}
