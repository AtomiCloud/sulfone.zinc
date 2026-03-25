using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class AddCommandsToTemplateVersions : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string[]>(
        name: "Commands",
        table: "TemplateVersions",
        type: "text[]",
        nullable: false,
        defaultValueSql: "'{}'"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "Commands", table: "TemplateVersions");
    }
  }
}
