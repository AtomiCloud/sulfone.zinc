using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class AddTemplateResolverUniqueIndex : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
        name: "IX_TemplateResolverVersions_TemplateId",
        table: "TemplateResolverVersions"
      );

      migrationBuilder.CreateIndex(
        name: "IX_TemplateResolverVersions_TemplateId_ResolverId",
        table: "TemplateResolverVersions",
        columns: new[] { "TemplateId", "ResolverId" },
        unique: true
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropIndex(
        name: "IX_TemplateResolverVersions_TemplateId_ResolverId",
        table: "TemplateResolverVersions"
      );

      migrationBuilder.CreateIndex(
        name: "IX_TemplateResolverVersions_TemplateId",
        table: "TemplateResolverVersions",
        column: "TemplateId"
      );
    }
  }
}
