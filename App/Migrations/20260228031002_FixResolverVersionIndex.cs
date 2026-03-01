using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class FixResolverVersionIndex : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(name: "IX_ResolverVersions_Id_Version", table: "ResolverVersions");

      migrationBuilder.DropIndex(name: "IX_ResolverVersions_ResolverId", table: "ResolverVersions");

      migrationBuilder.CreateIndex(
        name: "IX_ResolverVersions_ResolverId_Version",
        table: "ResolverVersions",
        columns: new[] { "ResolverId", "Version" },
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
        name: "IX_ResolverVersions_ResolverId_Version",
        table: "ResolverVersions"
      );

      migrationBuilder.CreateIndex(
        name: "IX_ResolverVersions_Id_Version",
        table: "ResolverVersions",
        columns: new[] { "Id", "Version" },
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_ResolverVersions_ResolverId",
        table: "ResolverVersions",
        column: "ResolverId"
      );
    }
  }
}
