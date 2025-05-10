using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class TemplateAllowEmptyProperty : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<bool>(
        name: "Empty",
        table: "TemplateVersions",
        type: "boolean",
        nullable: false,
        defaultValue: false
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "Empty", table: "TemplateVersions");
    }
  }
}
