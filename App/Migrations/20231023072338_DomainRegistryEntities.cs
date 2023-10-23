using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class DomainRegistryEntities : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
          name: "Plugins",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Downloads = table.Column<long>(type: "bigint", nullable: false),
            Name = table.Column<string>(type: "text", nullable: false),
            Project = table.Column<string>(type: "text", nullable: false),
            Source = table.Column<string>(type: "text", nullable: false),
            Email = table.Column<string>(type: "text", nullable: false),
            Tags = table.Column<string[]>(type: "text[]", nullable: false),
            Description = table.Column<string>(type: "text", nullable: false),
            Readme = table.Column<string>(type: "text", nullable: false),
            SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                  .Annotation("Npgsql:TsVectorConfig", "english")
                  .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "Description" }),
            UserId = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Plugins", x => x.Id);
            table.ForeignKey(
                      name: "FK_Plugins_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "Processors",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Downloads = table.Column<long>(type: "bigint", nullable: false),
            Name = table.Column<string>(type: "text", nullable: false),
            Project = table.Column<string>(type: "text", nullable: false),
            Source = table.Column<string>(type: "text", nullable: false),
            Email = table.Column<string>(type: "text", nullable: false),
            Tags = table.Column<string[]>(type: "text[]", nullable: false),
            Description = table.Column<string>(type: "text", nullable: false),
            Readme = table.Column<string>(type: "text", nullable: false),
            SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                  .Annotation("Npgsql:TsVectorConfig", "english")
                  .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "Description" }),
            UserId = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Processors", x => x.Id);
            table.ForeignKey(
                      name: "FK_Processors_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "Templates",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Downloads = table.Column<long>(type: "bigint", nullable: false),
            Name = table.Column<string>(type: "text", nullable: false),
            Project = table.Column<string>(type: "text", nullable: false),
            Source = table.Column<string>(type: "text", nullable: false),
            Email = table.Column<string>(type: "text", nullable: false),
            Tags = table.Column<string[]>(type: "text[]", nullable: false),
            Description = table.Column<string>(type: "text", nullable: false),
            Readme = table.Column<string>(type: "text", nullable: false),
            SearchVector = table.Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
                  .Annotation("Npgsql:TsVectorConfig", "english")
                  .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "Description" }),
            UserId = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_Templates", x => x.Id);
            table.ForeignKey(
                      name: "FK_Templates_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "PluginLikes",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            PluginId = table.Column<Guid>(type: "uuid", nullable: false),
            UserId = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PluginLikes", x => x.Id);
            table.ForeignKey(
                      name: "FK_PluginLikes_Plugins_PluginId",
                      column: x => x.PluginId,
                      principalTable: "Plugins",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_PluginLikes_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "PluginVersions",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Version = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            Description = table.Column<string>(type: "text", nullable: false),
            DockerReference = table.Column<string>(type: "text", nullable: false),
            DockerSha = table.Column<string>(type: "text", nullable: false),
            PluginId = table.Column<Guid>(type: "uuid", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_PluginVersions", x => x.Id);
            table.ForeignKey(
                      name: "FK_PluginVersions_Plugins_PluginId",
                      column: x => x.PluginId,
                      principalTable: "Plugins",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "ProcessorLikes",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            ProcessorId = table.Column<Guid>(type: "uuid", nullable: false),
            UserId = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ProcessorLikes", x => x.Id);
            table.ForeignKey(
                      name: "FK_ProcessorLikes_Processors_ProcessorId",
                      column: x => x.ProcessorId,
                      principalTable: "Processors",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_ProcessorLikes_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "ProcessorVersions",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Version = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            Description = table.Column<string>(type: "text", nullable: false),
            DockerReference = table.Column<string>(type: "text", nullable: false),
            DockerSha = table.Column<string>(type: "text", nullable: false),
            ProcessorId = table.Column<Guid>(type: "uuid", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_ProcessorVersions", x => x.Id);
            table.ForeignKey(
                      name: "FK_ProcessorVersions_Processors_ProcessorId",
                      column: x => x.ProcessorId,
                      principalTable: "Processors",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "TemplateLikes",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
            UserId = table.Column<string>(type: "text", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TemplateLikes", x => x.Id);
            table.ForeignKey(
                      name: "FK_TemplateLikes_Templates_TemplateId",
                      column: x => x.TemplateId,
                      principalTable: "Templates",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_TemplateLikes_Users_UserId",
                      column: x => x.UserId,
                      principalTable: "Users",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "TemplateVersions",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            Version = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
            CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            Description = table.Column<string>(type: "text", nullable: false),
            BlobDockerReference = table.Column<string>(type: "text", nullable: false),
            BlobDockerSha = table.Column<string>(type: "text", nullable: false),
            TemplateDockerReference = table.Column<string>(type: "text", nullable: false),
            TemplateDockerSha = table.Column<string>(type: "text", nullable: false),
            TemplateId = table.Column<Guid>(type: "uuid", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TemplateVersions", x => x.Id);
            table.ForeignKey(
                      name: "FK_TemplateVersions_Templates_TemplateId",
                      column: x => x.TemplateId,
                      principalTable: "Templates",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "TemplatePluginVersions",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
            PluginId = table.Column<Guid>(type: "uuid", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TemplatePluginVersions", x => x.Id);
            table.ForeignKey(
                      name: "FK_TemplatePluginVersions_PluginVersions_PluginId",
                      column: x => x.PluginId,
                      principalTable: "PluginVersions",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_TemplatePluginVersions_TemplateVersions_TemplateId",
                      column: x => x.TemplateId,
                      principalTable: "TemplateVersions",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateTable(
          name: "TemplateProcessorVersions",
          columns: table => new
          {
            Id = table.Column<Guid>(type: "uuid", nullable: false),
            TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
            ProcessorId = table.Column<Guid>(type: "uuid", nullable: false)
          },
          constraints: table =>
          {
            table.PrimaryKey("PK_TemplateProcessorVersions", x => x.Id);
            table.ForeignKey(
                      name: "FK_TemplateProcessorVersions_ProcessorVersions_ProcessorId",
                      column: x => x.ProcessorId,
                      principalTable: "ProcessorVersions",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
            table.ForeignKey(
                      name: "FK_TemplateProcessorVersions_TemplateVersions_TemplateId",
                      column: x => x.TemplateId,
                      principalTable: "TemplateVersions",
                      principalColumn: "Id",
                      onDelete: ReferentialAction.Cascade);
          });

      migrationBuilder.CreateIndex(
          name: "IX_PluginLikes_PluginId",
          table: "PluginLikes",
          column: "PluginId");

      migrationBuilder.CreateIndex(
          name: "IX_PluginLikes_UserId_PluginId",
          table: "PluginLikes",
          columns: new[] { "UserId", "PluginId" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_PluginVersions_Id_Version",
          table: "PluginVersions",
          columns: new[] { "Id", "Version" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_PluginVersions_PluginId",
          table: "PluginVersions",
          column: "PluginId");

      migrationBuilder.CreateIndex(
          name: "IX_Plugins_SearchVector",
          table: "Plugins",
          column: "SearchVector")
          .Annotation("Npgsql:IndexMethod", "GIN");

      migrationBuilder.CreateIndex(
          name: "IX_Plugins_UserId_Name",
          table: "Plugins",
          columns: new[] { "UserId", "Name" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_ProcessorLikes_ProcessorId",
          table: "ProcessorLikes",
          column: "ProcessorId");

      migrationBuilder.CreateIndex(
          name: "IX_ProcessorLikes_UserId_ProcessorId",
          table: "ProcessorLikes",
          columns: new[] { "UserId", "ProcessorId" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_ProcessorVersions_Id_Version",
          table: "ProcessorVersions",
          columns: new[] { "Id", "Version" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_ProcessorVersions_ProcessorId",
          table: "ProcessorVersions",
          column: "ProcessorId");

      migrationBuilder.CreateIndex(
          name: "IX_Processors_SearchVector",
          table: "Processors",
          column: "SearchVector")
          .Annotation("Npgsql:IndexMethod", "GIN");

      migrationBuilder.CreateIndex(
          name: "IX_Processors_UserId_Name",
          table: "Processors",
          columns: new[] { "UserId", "Name" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_TemplateLikes_TemplateId",
          table: "TemplateLikes",
          column: "TemplateId");

      migrationBuilder.CreateIndex(
          name: "IX_TemplateLikes_UserId_TemplateId",
          table: "TemplateLikes",
          columns: new[] { "UserId", "TemplateId" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_TemplatePluginVersions_PluginId",
          table: "TemplatePluginVersions",
          column: "PluginId");

      migrationBuilder.CreateIndex(
          name: "IX_TemplatePluginVersions_TemplateId",
          table: "TemplatePluginVersions",
          column: "TemplateId");

      migrationBuilder.CreateIndex(
          name: "IX_TemplateProcessorVersions_ProcessorId",
          table: "TemplateProcessorVersions",
          column: "ProcessorId");

      migrationBuilder.CreateIndex(
          name: "IX_TemplateProcessorVersions_TemplateId",
          table: "TemplateProcessorVersions",
          column: "TemplateId");

      migrationBuilder.CreateIndex(
          name: "IX_TemplateVersions_Id_Version",
          table: "TemplateVersions",
          columns: new[] { "Id", "Version" },
          unique: true);

      migrationBuilder.CreateIndex(
          name: "IX_TemplateVersions_TemplateId",
          table: "TemplateVersions",
          column: "TemplateId");

      migrationBuilder.CreateIndex(
          name: "IX_Templates_SearchVector",
          table: "Templates",
          column: "SearchVector")
          .Annotation("Npgsql:IndexMethod", "GIN");

      migrationBuilder.CreateIndex(
          name: "IX_Templates_UserId_Name",
          table: "Templates",
          columns: new[] { "UserId", "Name" },
          unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(
          name: "PluginLikes");

      migrationBuilder.DropTable(
          name: "ProcessorLikes");

      migrationBuilder.DropTable(
          name: "TemplateLikes");

      migrationBuilder.DropTable(
          name: "TemplatePluginVersions");

      migrationBuilder.DropTable(
          name: "TemplateProcessorVersions");

      migrationBuilder.DropTable(
          name: "PluginVersions");

      migrationBuilder.DropTable(
          name: "ProcessorVersions");

      migrationBuilder.DropTable(
          name: "TemplateVersions");

      migrationBuilder.DropTable(
          name: "Plugins");

      migrationBuilder.DropTable(
          name: "Processors");

      migrationBuilder.DropTable(
          name: "Templates");
    }
  }
}
