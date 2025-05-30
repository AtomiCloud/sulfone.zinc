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
          Id = table.Column<string>(type: "text", nullable: false),
          Username = table.Column<string>(type: "text", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Users", x => x.Id);
        }
      );

      migrationBuilder.CreateTable(
        name: "Tokens",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          Name = table.Column<string>(type: "text", nullable: false),
          ApiToken = table.Column<string>(type: "text", nullable: false),
          Revoked = table.Column<bool>(type: "boolean", nullable: false),
          UserId = table.Column<string>(type: "text", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Tokens", x => x.Id);
          table.ForeignKey(
            name: "FK_Tokens_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_Tokens_ApiToken",
        table: "Tokens",
        column: "ApiToken",
        unique: true
      );

      migrationBuilder.CreateIndex(name: "IX_Tokens_UserId", table: "Tokens", column: "UserId");

      migrationBuilder.CreateIndex(
        name: "IX_Users_Username",
        table: "Users",
        column: "Username",
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "Tokens");

      migrationBuilder.DropTable(name: "Users");
    }
  }
}
