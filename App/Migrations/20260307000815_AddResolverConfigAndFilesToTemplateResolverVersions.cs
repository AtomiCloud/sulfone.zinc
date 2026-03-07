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
      migrationBuilder.AddColumn<string>(
        name: "Config",
        table: "TemplateResolverVersions",
        type: "text",
        nullable: false,
        defaultValue: ""
      );

      migrationBuilder.AddColumn<string[]>(
        name: "Files",
        table: "TemplateResolverVersions",
        type: "text[]",
        nullable: false,
        defaultValue: new string[0]
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
