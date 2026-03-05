using System.Text.Json;
using App.Modules.Cyan.API.V1.Mappers;
using App.Modules.Cyan.API.V1.Models;
using App.Modules.Cyan.API.V1.Validators;
using Domain.Model;
using FluentValidation.TestHelper;

namespace IntTest;

/// <summary>
/// Integration tests for resolver config/files flow through API -> Domain layers.
/// Tests the complete mapping pipeline from ResolverReferenceReq -> TemplateVersionResolverInput.
/// </summary>
public class ResolverIntegrationTests
{
  /// <summary>
  /// Tests the complete flow for creating a template version with resolvers.
  /// This simulates what happens when a client POSTs to /api/v1/template/{username}/{name}/versions
  /// with resolver config and files.
  /// </summary>
  [Fact]
  public void CreateTemplateVersion_WithResolvers_MapsThroughAllLayers()
  {
    // Arrange - Simulate the API request body for creating a template version
    var resolverConfig = JsonDocument.Parse(
      """{"strategy": "deep-merge", "preserveArrays": true}"""
    );
    var resolverFiles = new[] { "package.json", "**/tsconfig.json", "src/**/*.yaml" };

    var createReq = new CreateTemplateVersionReq(
      Description: "Version with resolvers",
      Properties: null,
      Plugins: Array.Empty<PluginReferenceReq>(),
      Processors: Array.Empty<ProcessorReferenceReq>(),
      Templates: Array.Empty<TemplateReferenceReq>(),
      Resolvers: new[]
      {
        new ResolverReferenceReq(
          "atomi",
          "json-merger",
          1,
          resolverConfig.RootElement,
          resolverFiles
        ),
        new ResolverReferenceReq(
          "atomi",
          "yaml-merger",
          2,
          JsonDocument.Parse("""{"indent": 2}""").RootElement,
          new[] { "*.yaml", "*.yml" }
        ),
      }
    );

    // Act - Map the resolvers from API model to service input (what the controller does)
    var mappedResolvers = createReq.Resolvers.Select(r => r.ToDomain()).ToList();

    // Assert - Verify all fields mapped correctly through the pipeline
    mappedResolvers.Should().HaveCount(2);

    // First resolver
    var firstResolver = mappedResolvers[0];
    firstResolver.Resolver.Username.Should().Be("atomi");
    firstResolver.Resolver.Name.Should().Be("json-merger");
    firstResolver.Resolver.Version.Should().Be(1);
    firstResolver.Config.TryGetProperty("strategy", out var strategy).Should().BeTrue();
    strategy.GetString().Should().Be("deep-merge");
    firstResolver.Files.Should().BeEquivalentTo(resolverFiles);

    // Second resolver
    var secondResolver = mappedResolvers[1];
    secondResolver.Resolver.Username.Should().Be("atomi");
    secondResolver.Resolver.Name.Should().Be("yaml-merger");
    secondResolver.Resolver.Version.Should().Be(2);
    secondResolver.Config.TryGetProperty("indent", out var indent).Should().BeTrue();
    indent.GetInt32().Should().Be(2);
    secondResolver.Files.Should().BeEquivalentTo(new[] { "*.yaml", "*.yml" });
  }

  /// <summary>
  /// Tests the push template flow with resolvers.
  /// This simulates what happens when a client POSTs to /api/v1/template/push/{username}
  /// with resolver config and files.
  /// </summary>
  [Fact]
  public void PushTemplate_WithResolvers_MapsThroughAllLayers()
  {
    // Arrange - Simulate the API request body for pushing a template
    var pushReq = new PushTemplateReq(
      Name: "my-pipeline",
      Project: "my-project",
      Source: "github.com/user/repo",
      Email: "user@example.com",
      Tags: new[] { "ci", "docker" },
      Description: "My CI/CD pipeline",
      Readme: "# Usage\n...",
      VersionDescription: "Initial version",
      Properties: null,
      Plugins: Array.Empty<PluginReferenceReq>(),
      Processors: Array.Empty<ProcessorReferenceReq>(),
      Templates: Array.Empty<TemplateReferenceReq>(),
      Resolvers: new[]
      {
        new ResolverReferenceReq(
          "atomi",
          "env-injector",
          1,
          JsonDocument.Parse("""{"envPrefix": "MY_APP_"}""").RootElement,
          new[] { "**/.env*" }
        ),
      }
    );

    // Act - Map the resolvers from API model to service input
    var mappedResolvers = pushReq.Resolvers.Select(r => r.ToDomain()).ToList();

    // Assert
    mappedResolvers.Should().HaveCount(1);
    var resolver = mappedResolvers[0];
    resolver.Resolver.Username.Should().Be("atomi");
    resolver.Resolver.Name.Should().Be("env-injector");
    resolver.Resolver.Version.Should().Be(1);
    resolver.Config.TryGetProperty("envPrefix", out var envPrefix).Should().BeTrue();
    envPrefix.GetString().Should().Be("MY_APP_");
    resolver.Files.Should().BeEquivalentTo(new[] { "**/.env*" });
  }

  /// <summary>
  /// Tests that version 0 maps to null (latest version lookup).
  /// </summary>
  [Fact]
  public void ResolverReference_VersionZero_MapsToLatest()
  {
    // Arrange
    var req = new ResolverReferenceReq(
      "atomi",
      "json-merger",
      0, // 0 means "use latest"
      JsonDocument.Parse("{}").RootElement,
      Array.Empty<string>()
    );

    // Act
    var result = req.ToDomain();

    // Assert
    result.Resolver.Version.Should().BeNull("version 0 should map to null indicating latest");
  }

  /// <summary>
  /// Tests validation of resolver references in create template version request.
  /// </summary>
  [Fact]
  public void CreateTemplateVersion_WithInvalidResolverFiles_FailsValidation()
  {
    // Arrange - resolver with null Files (invalid)
    var createReq = new CreateTemplateVersionReq(
      Description: "Version with invalid resolver",
      Properties: null,
      Plugins: Array.Empty<PluginReferenceReq>(),
      Processors: Array.Empty<ProcessorReferenceReq>(),
      Templates: Array.Empty<TemplateReferenceReq>(),
      Resolvers: new[]
      {
        new ResolverReferenceReq(
          "atomi",
          "json-merger",
          1,
          JsonDocument.Parse("{}").RootElement,
          null! // Invalid - Files cannot be null
        ),
      }
    );

    var validator = new CreateTemplateVersionReqValidator();

    // Act
    var result = validator.TestValidate(createReq);

    // Assert
    result.ShouldHaveValidationErrorFor("Resolvers[0].Files");
  }

  /// <summary>
  /// Tests response mapping from domain to API response.
  /// Verifies the ToTemplateResolverResp mapper produces correct output.
  /// </summary>
  [Fact]
  public void TemplateVersionResolver_MapsToResponseCorrectly()
  {
    // Arrange - Create a domain TemplateVersionResolverRef
    var configJson = JsonDocument.Parse("""{"key": "value"}""").RootElement;
    var files = new[] { "file1.json" };
    var createdAt = new DateTime(2024, 6, 1, 12, 0, 0, DateTimeKind.Utc);
    var resolverId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    var resolverRef = new TemplateVersionResolverRef(
      new ResolverVersionPrincipal
      {
        Id = resolverId,
        Version = 5,
        CreatedAt = createdAt,
        Record = new ResolverVersionRecord { Description = "Test resolver" },
        Property = new ResolverVersionProperty
        {
          DockerReference = "atomi/test-resolver",
          DockerTag = "v5",
        },
      },
      configJson,
      files
    );

    // Act
    var response = resolverRef.ToTemplateResolverResp();

    // Assert
    response.Id.Should().Be(resolverId);
    response.Version.Should().Be(5);
    response.CreatedAt.Should().Be(createdAt);
    response.Description.Should().Be("Test resolver");
    response.DockerReference.Should().Be("atomi/test-resolver");
    response.DockerTag.Should().Be("v5");
    response.Config.ToString().Should().Be(configJson.ToString());
    response.Files.Should().BeEquivalentTo(files);
  }
}
