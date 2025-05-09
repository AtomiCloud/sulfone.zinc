using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class TemplateListForTemplate : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "TemplateTemplateVersions",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
          TemplateRefId = table.Column<Guid>(type: "uuid", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TemplateTemplateVersions", x => x.Id);
          table.ForeignKey(
            name: "FK_TemplateTemplateVersions_TemplateVersions_TemplateId",
            column: x => x.TemplateId,
            principalTable: "TemplateVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_TemplateTemplateVersions_TemplateVersions_TemplateRefId",
            column: x => x.TemplateRefId,
            principalTable: "TemplateVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_TemplateTemplateVersions_TemplateId",
        table: "TemplateTemplateVersions",
        column: "TemplateId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_TemplateTemplateVersions_TemplateRefId",
        table: "TemplateTemplateVersions",
        column: "TemplateRefId"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "TemplateTemplateVersions");
    }
  }
}
