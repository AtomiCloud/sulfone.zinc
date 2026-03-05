using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class AddResolverConfigAndFilesToTemplateResolverVersions : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      // Add Config column as TEXT with default '{}' (JSON string)
      migrationBuilder.AddColumn<string>(
        name: "Config",
        table: "TemplateResolverVersions",
        type: "text",
        nullable: false,
        defaultValue: "{}"
      );

      // Add Files column as TEXT[] with default empty array
      migrationBuilder.AddColumn<string[]>(
        name: "Files",
        table: "TemplateResolverVersions",
        type: "text[]",
        nullable: false,
        defaultValueSql: "ARRAY[]::text[]"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "Config", table: "TemplateResolverVersions");

      migrationBuilder.DropColumn(name: "Files", table: "TemplateResolverVersions");
    }
  }
}
