using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class MigrateToTagFromSha : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
        name: "TemplateDockerSha",
        table: "TemplateVersions",
        newName: "TemplateDockerTag"
      );

      migrationBuilder.RenameColumn(
        name: "BlobDockerSha",
        table: "TemplateVersions",
        newName: "BlobDockerTag"
      );

      migrationBuilder.RenameColumn(
        name: "DockerSha",
        table: "ProcessorVersions",
        newName: "DockerTag"
      );

      migrationBuilder.RenameColumn(
        name: "DockerSha",
        table: "PluginVersions",
        newName: "DockerTag"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.RenameColumn(
        name: "TemplateDockerTag",
        table: "TemplateVersions",
        newName: "TemplateDockerSha"
      );

      migrationBuilder.RenameColumn(
        name: "BlobDockerTag",
        table: "TemplateVersions",
        newName: "BlobDockerSha"
      );

      migrationBuilder.RenameColumn(
        name: "DockerTag",
        table: "ProcessorVersions",
        newName: "DockerSha"
      );

      migrationBuilder.RenameColumn(
        name: "DockerTag",
        table: "PluginVersions",
        newName: "DockerSha"
      );
    }
  }
}
