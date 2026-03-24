using System.Text.Json;
using App.Modules.Cyan.API.V1.Mappers;
using App.Modules.Cyan.API.V1.Models;
using Domain.Model;
using Domain.Service;
using FluentAssertions;

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

  #region ToDomain(TemplateReferenceReq) Tests

  [Fact]
  public void ToDomain_TemplateReferenceReq_MapsAllFieldsCorrectly()
  {
    // Arrange
    var presetAnswers = JsonDocument.Parse("""{"projectName": "my-app", "author": "test"}""").RootElement;
    var req = new TemplateReferenceReq("atomi", "base-template", 3, presetAnswers);

    // Act
    var result = req.ToDomain();

    // Assert
    result.Should().NotBeNull();
    result.Template.Username.Should().Be("atomi");
    result.Template.Name.Should().Be("base-template");
    result.Template.Version.Should().Be(3);
    result.PresetAnswers.ToString().Should().Be(presetAnswers.ToString());
  }

  [Fact]
  public void ToDomain_TemplateReferenceReq_VersionZero_BecomesNull()
  {
    // Arrange
    var presetAnswers = JsonDocument.Parse("{}").RootElement;
    var req = new TemplateReferenceReq("atomi", "base-template", 0, presetAnswers);

    // Act
    var result = req.ToDomain();

    // Assert
    result.Template.Version.Should().BeNull("version 0 should map to null (latest)");
  }

  [Fact]
  public void ToDomain_TemplateReferenceReq_EmptyPresetAnswers_ArePreserved()
  {
    // Arrange
    var presetAnswers = JsonDocument.Parse("{}").RootElement;
    var req = new TemplateReferenceReq("user", "template", 1, presetAnswers);

    // Act
    var result = req.ToDomain();

    // Assert
    result.PresetAnswers.ValueKind.Should().Be(JsonValueKind.Object);
  }

  [Fact]
  public void ToDomain_TemplateReferenceReq_ArrayPresetAnswers_ArePreserved()
  {
    // Arrange
    var presetAnswers = JsonDocument.Parse("""{"tags": ["go", "web"], "enabled": true}""").RootElement;
    var req = new TemplateReferenceReq("user", "template", 2, presetAnswers);

    // Act
    var result = req.ToDomain();

    // Assert
    result.PresetAnswers.TryGetProperty("tags", out var tags).Should().BeTrue();
    tags.GetArrayLength().Should().Be(2);
    result.PresetAnswers.TryGetProperty("enabled", out var enabled).Should().BeTrue();
    enabled.GetBoolean().Should().BeTrue();
  }

  #endregion

  #region ToTemplateRefResp Tests

  [Fact]
  public void ToTemplateRefResp_MapsAllFieldsCorrectly()
  {
    // Arrange
    var presetAnswers = JsonDocument.Parse("""{"name": "my-project"}""").RootElement;
    var createdAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

    var templateRef = new TemplateVersionTemplateRef(
      new TemplateVersionPrincipal
      {
        Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
        Version = 2,
        CreatedAt = createdAt,
        Record = new TemplateVersionRecord { Description = "Test sub-template description" },
        Property = null,
      },
      presetAnswers
    );

    // Act
    var result = templateRef.ToTemplateRefResp();

    // Assert
    result.Id.Should().Be(Guid.Parse("12345678-1234-1234-1234-123456789012"));
    result.Version.Should().Be(2);
    result.CreatedAt.Should().Be(createdAt);
    result.Description.Should().Be("Test sub-template description");
    result.Properties.Should().BeNull();
    result.PresetAnswers.ToString().Should().Be(presetAnswers.ToString());
  }

  [Fact]
  public void ToTemplateRefResp_WithEmptyPresetAnswers_MapsCorrectly()
  {
    // Arrange
    var presetAnswers = JsonDocument.Parse("{}").RootElement;

    var templateRef = new TemplateVersionTemplateRef(
      new TemplateVersionPrincipal
      {
        Id = Guid.NewGuid(),
        Version = 1,
        CreatedAt = DateTime.UtcNow,
        Record = new TemplateVersionRecord { Description = "Empty preset answers" },
        Property = null,
      },
      presetAnswers
    );

    // Act
    var result = templateRef.ToTemplateRefResp();

    // Assert
    result.PresetAnswers.ValueKind.Should().Be(JsonValueKind.Object);
  }

  [Fact]
  public void ToTemplateRefResp_WithComplexPresetAnswers_MapsCorrectly()
  {
    // Arrange
    var presetAnswers = JsonDocument
      .Parse("""{"nested": {"key": "value"}, "array": [1, 2, 3], "enabled": true, "name": "test"}""")
      .RootElement;

    var templateRef = new TemplateVersionTemplateRef(
      new TemplateVersionPrincipal
      {
        Id = Guid.NewGuid(),
        Version = 1,
        CreatedAt = DateTime.UtcNow,
        Record = new TemplateVersionRecord { Description = "Complex preset answers" },
        Property = null,
      },
      presetAnswers
    );

    // Act
    var result = templateRef.ToTemplateRefResp();

    // Assert
    result.PresetAnswers.TryGetProperty("nested", out var nested).Should().BeTrue();
    nested.TryGetProperty("key", out var key).Should().BeTrue();
    key.GetString().Should().Be("value");
    result.PresetAnswers.TryGetProperty("enabled", out var enabled).Should().BeTrue();
    enabled.GetBoolean().Should().BeTrue();
    result.PresetAnswers.TryGetProperty("array", out var array).Should().BeTrue();
    array.GetArrayLength().Should().Be(3);
  }

  #endregion
}
