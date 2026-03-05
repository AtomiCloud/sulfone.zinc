using System.Text.Json;
using App.Modules.Cyan.API.V1.Mappers;
using App.Modules.Cyan.API.V1.Models;
using Domain.Model;
using Domain.Service;

namespace UnitTest;

public class TemplateVersionMapperTests
{
  #region ToDomain(ResolverReferenceReq) Tests

  [Fact]
  public void ToDomain_ResolverReferenceReq_MapsAllFieldsCorrectly()
  {
    // Arrange
    var configJson = JsonDocument.Parse("""{"strategy": "deep-merge"}""").RootElement;
    var files = new[] { "package.json", "**/tsconfig.json" };
    var req = new ResolverReferenceReq("atomi", "json-merger", 5, configJson, files);

    // Act
    var result = req.ToDomain();

    // Assert
    result.Should().NotBeNull();
    result.Resolver.Username.Should().Be("atomi");
    result.Resolver.Name.Should().Be("json-merger");
    result.Resolver.Version.Should().Be(5);
    result.Config.ToString().Should().Be(configJson.ToString());
    result.Files.Should().BeEquivalentTo(files);
  }

  [Fact]
  public void ToDomain_ResolverReferenceReq_VersionZero_BecomesNull()
  {
    // Arrange
    var configJson = JsonDocument.Parse("{}").RootElement;
    var files = Array.Empty<string>();
    var req = new ResolverReferenceReq("atomi", "json-merger", 0, configJson, files);

    // Act
    var result = req.ToDomain();

    // Assert
    result.Resolver.Version.Should().BeNull("version 0 should map to null (latest)");
  }

  [Fact]
  public void ToDomain_ResolverReferenceReq_EmptyConfigAndFiles_ArePreserved()
  {
    // Arrange
    var configJson = JsonDocument.Parse("{}").RootElement;
    var files = Array.Empty<string>();
    var req = new ResolverReferenceReq("user", "resolver", 1, configJson, files);

    // Act
    var result = req.ToDomain();

    // Assert
    result.Config.ValueKind.Should().Be(JsonValueKind.Object);
    result.Files.Should().BeEmpty();
  }

  #endregion

  #region ToTemplateResolverResp Tests

  [Fact]
  public void ToTemplateResolverResp_MapsAllFieldsCorrectly()
  {
    // Arrange
    var configJson = JsonDocument.Parse("""{"key": "value"}""").RootElement;
    var files = new[] { "file1.json", "file2.yaml" };
    var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    var resolverRef = new TemplateVersionResolverRef(
      new ResolverVersionPrincipal
      {
        Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
        Version = 3,
        CreatedAt = createdAt,
        Record = new ResolverVersionRecord { Description = "Test resolver description" },
        Property = new ResolverVersionProperty
        {
          DockerReference = "atomi/test-resolver",
          DockerTag = "v3",
        },
      },
      configJson,
      files
    );

    // Act
    var result = resolverRef.ToTemplateResolverResp();

    // Assert
    result.Id.Should().Be(Guid.Parse("12345678-1234-1234-1234-123456789012"));
    result.Version.Should().Be(3);
    result.CreatedAt.Should().Be(createdAt);
    result.Description.Should().Be("Test resolver description");
    result.DockerReference.Should().Be("atomi/test-resolver");
    result.DockerTag.Should().Be("v3");
    result.Config.ToString().Should().Be(configJson.ToString());
    result.Files.Should().BeEquivalentTo(files);
  }

  [Fact]
  public void ToTemplateResolverResp_WithEmptyConfigAndFiles_MapsCorrectly()
  {
    // Arrange
    var configJson = JsonDocument.Parse("{}").RootElement;
    var files = Array.Empty<string>();

    var resolverRef = new TemplateVersionResolverRef(
      new ResolverVersionPrincipal
      {
        Id = Guid.NewGuid(),
        Version = 1,
        CreatedAt = DateTime.UtcNow,
        Record = new ResolverVersionRecord { Description = "Empty resolver" },
        Property = new ResolverVersionProperty
        {
          DockerReference = "test/empty",
          DockerTag = "latest",
        },
      },
      configJson,
      files
    );

    // Act
    var result = resolverRef.ToTemplateResolverResp();

    // Assert
    result.Config.ValueKind.Should().Be(JsonValueKind.Object);
    result.Files.Should().BeEmpty();
  }

  [Fact]
  public void ToTemplateResolverResp_WithComplexConfig_MapsCorrectly()
  {
    // Arrange
    var configJson = JsonDocument
      .Parse("""{"nested": {"deep": "value"}, "array": [1, 2, 3], "string": "test"}""")
      .RootElement;
    var files = new[] { "**/*.json" };

    var resolverRef = new TemplateVersionResolverRef(
      new ResolverVersionPrincipal
      {
        Id = Guid.NewGuid(),
        Version = 1,
        CreatedAt = DateTime.UtcNow,
        Record = new ResolverVersionRecord { Description = "Complex config" },
        Property = new ResolverVersionProperty
        {
          DockerReference = "test/complex",
          DockerTag = "1.0",
        },
      },
      configJson,
      files
    );

    // Act
    var result = resolverRef.ToTemplateResolverResp();

    // Assert
    result.Config.TryGetProperty("nested", out var nested).Should().BeTrue();
    nested.TryGetProperty("deep", out var deep).Should().BeTrue();
    deep.GetString().Should().Be("value");
    result.Files.Should().Contain("**/*.json");
  }

  #endregion
}
