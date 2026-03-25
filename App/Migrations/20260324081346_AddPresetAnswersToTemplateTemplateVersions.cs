using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class AddPresetAnswersToTemplateTemplateVersions : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.AddColumn<string>(
        name: "PresetAnswers",
        table: "TemplateTemplateVersions",
        type: "text",
        nullable: false,
        defaultValue: "{}"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropColumn(name: "PresetAnswers", table: "TemplateTemplateVersions");
    }
  }
}
