using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace App.Migrations
{
  /// <inheritdoc />
  public partial class AddResolvers : Migration
  {
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.CreateTable(
        name: "Resolvers",
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
          SearchVector = table
            .Column<NpgsqlTsVector>(type: "tsvector", nullable: false)
            .Annotation("Npgsql:TsVectorConfig", "english")
            .Annotation("Npgsql:TsVectorProperties", new[] { "Name", "Description" }),
          UserId = table.Column<string>(type: "text", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_Resolvers", x => x.Id);
          table.ForeignKey(
            name: "FK_Resolvers_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "ResolverLikes",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          ResolverId = table.Column<Guid>(type: "uuid", nullable: false),
          UserId = table.Column<string>(type: "text", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ResolverLikes", x => x.Id);
          table.ForeignKey(
            name: "FK_ResolverLikes_Resolvers_ResolverId",
            column: x => x.ResolverId,
            principalTable: "Resolvers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_ResolverLikes_Users_UserId",
            column: x => x.UserId,
            principalTable: "Users",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "ResolverVersions",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          Version = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
          CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
          Description = table.Column<string>(type: "text", nullable: false),
          DockerReference = table.Column<string>(type: "text", nullable: false),
          DockerTag = table.Column<string>(type: "text", nullable: false),
          ResolverId = table.Column<Guid>(type: "uuid", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_ResolverVersions", x => x.Id);
          table.ForeignKey(
            name: "FK_ResolverVersions_Resolvers_ResolverId",
            column: x => x.ResolverId,
            principalTable: "Resolvers",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateTable(
        name: "TemplateResolverVersions",
        columns: table => new
        {
          Id = table.Column<Guid>(type: "uuid", nullable: false),
          TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
          ResolverId = table.Column<Guid>(type: "uuid", nullable: false),
        },
        constraints: table =>
        {
          table.PrimaryKey("PK_TemplateResolverVersions", x => x.Id);
          table.ForeignKey(
            name: "FK_TemplateResolverVersions_ResolverVersions_ResolverId",
            column: x => x.ResolverId,
            principalTable: "ResolverVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
          table.ForeignKey(
            name: "FK_TemplateResolverVersions_TemplateVersions_TemplateId",
            column: x => x.TemplateId,
            principalTable: "TemplateVersions",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade
          );
        }
      );

      migrationBuilder.CreateIndex(
        name: "IX_ResolverLikes_ResolverId",
        table: "ResolverLikes",
        column: "ResolverId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_ResolverLikes_UserId_ResolverId",
        table: "ResolverLikes",
        columns: new[] { "UserId", "ResolverId" },
        unique: true
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

      migrationBuilder
        .CreateIndex(name: "IX_Resolvers_SearchVector", table: "Resolvers", column: "SearchVector")
        .Annotation("Npgsql:IndexMethod", "GIN");

      migrationBuilder.CreateIndex(
        name: "IX_Resolvers_UserId_Name",
        table: "Resolvers",
        columns: new[] { "UserId", "Name" },
        unique: true
      );

      migrationBuilder.CreateIndex(
        name: "IX_TemplateResolverVersions_ResolverId",
        table: "TemplateResolverVersions",
        column: "ResolverId"
      );

      migrationBuilder.CreateIndex(
        name: "IX_TemplateResolverVersions_TemplateId",
        table: "TemplateResolverVersions",
        column: "TemplateId"
      );
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
      migrationBuilder.DropTable(name: "ResolverLikes");

      migrationBuilder.DropTable(name: "TemplateResolverVersions");

      migrationBuilder.DropTable(name: "ResolverVersions");

      migrationBuilder.DropTable(name: "Resolvers");
    }
  }
}
