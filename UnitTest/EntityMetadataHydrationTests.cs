using App.Modules.Cyan.Data.Mappers;
using App.Modules.Cyan.Data.Models;
using Domain.Model;
using FluentAssertions;

namespace UnitTest;

/// <summary>
/// Regression tests verifying that entity metadata (including Readme) is correctly
/// hydrated when pushing updates to existing plugins, processors, and resolvers.
/// These lock in the expectation that repeated pushes update top-level metadata.
/// </summary>
public class EntityMetadataHydrationTests
{
  private static PluginMetadata NewPluginMetadata(string readme) => new()
  {
    Project = "updated-project",
    Source = "updated-source",
    Email = "updated@email.com",
    Tags = ["updated-tag"],
    Description = "updated-description",
    Readme = readme,
  };

  private static ProcessorMetadata NewProcessorMetadata(string readme) => new()
  {
    Project = "updated-project",
    Source = "updated-source",
    Email = "updated@email.com",
    Tags = ["updated-tag"],
    Description = "updated-description",
    Readme = readme,
  };

  private static ResolverMetadata NewResolverMetadata(string readme) => new()
  {
    Project = "updated-project",
    Source = "updated-source",
    Email = "updated@email.com",
    Tags = ["updated-tag"],
    Description = "updated-description",
    Readme = readme,
  };

  private static TemplateMetadata NewTemplateMetadata(string readme) => new()
  {
    Project = "updated-project",
    Source = "updated-source",
    Email = "updated@email.com",
    Tags = ["updated-tag"],
    Description = "updated-description",
    Readme = readme,
  };

  [Fact]
  public void PluginData_HydrateData_UpdatesReadme()
  {
    var original = new PluginData { Readme = "original-readme" };
    var updated = original.HydrateData(NewPluginMetadata("new-readme-content"));
    updated.Readme.Should().Be("new-readme-content");
  }

  [Fact]
  public void PluginData_HydrateData_RepeatedPush_ReflectsLatestReadme()
  {
    var data = new PluginData { Readme = "v1-readme" };
    data = data.HydrateData(NewPluginMetadata("v2-readme"));
    data = data.HydrateData(NewPluginMetadata("v3-readme"));
    data.Readme.Should().Be("v3-readme");
  }

  [Fact]
  public void PluginData_HydrateData_PreservesRecordFields()
  {
    var data = new PluginData
    {
      Id = Guid.NewGuid(),
      Name = "my-plugin",
      UserId = "user-123",
      Downloads = 42,
    };
    var updated = data.HydrateData(NewPluginMetadata("updated-readme"));
    updated.Id.Should().Be(data.Id);
    updated.Name.Should().Be(data.Name);
    updated.UserId.Should().Be(data.UserId);
    updated.Downloads.Should().Be(data.Downloads);
  }

  [Fact]
  public void ProcessorData_HydrateData_UpdatesReadme()
  {
    var original = new ProcessorData { Readme = "original-readme" };
    var updated = original.HydrateData(NewProcessorMetadata("new-readme-content"));
    updated.Readme.Should().Be("new-readme-content");
  }

  [Fact]
  public void ProcessorData_HydrateData_RepeatedPush_ReflectsLatestReadme()
  {
    var data = new ProcessorData { Readme = "v1-readme" };
    data = data.HydrateData(NewProcessorMetadata("v2-readme"));
    data = data.HydrateData(NewProcessorMetadata("v3-readme"));
    data.Readme.Should().Be("v3-readme");
  }

  [Fact]
  public void ResolverData_HydrateData_UpdatesReadme()
  {
    var original = new ResolverData { Readme = "original-readme" };
    var updated = original.HydrateData(NewResolverMetadata("new-readme-content"));
    updated.Readme.Should().Be("new-readme-content");
  }

  [Fact]
  public void ResolverData_HydrateData_RepeatedPush_ReflectsLatestReadme()
  {
    var data = new ResolverData { Readme = "v1-readme" };
    data = data.HydrateData(NewResolverMetadata("v2-readme"));
    data = data.HydrateData(NewResolverMetadata("v3-readme"));
    data.Readme.Should().Be("v3-readme");
  }

  [Fact]
  public void TemplateData_HydrateData_UpdatesReadme()
  {
    var original = new TemplateData { Readme = "original-readme" };
    var updated = original.HydrateData(NewTemplateMetadata("new-readme-content"));
    updated.Readme.Should().Be("new-readme-content");
  }

  [Fact]
  public void TemplateData_HydrateData_RepeatedPush_ReflectsLatestReadme()
  {
    var data = new TemplateData { Readme = "v1-readme" };
    data = data.HydrateData(NewTemplateMetadata("v2-readme"));
    data = data.HydrateData(NewTemplateMetadata("v3-readme"));
    data.Readme.Should().Be("v3-readme");
  }

  [Fact]
  public void PluginData_ToPrincipal_RoundTripsReadme()
  {
    var data = new PluginData
    {
      Id = Guid.NewGuid(),
      UserId = "user-123",
      Name = "plugin",
      Readme = "readme-content",
    };
    var principal = data.ToPrincipal();
    principal.Metadata.Readme.Should().Be("readme-content");
  }

  [Fact]
  public void ProcessorData_ToPrincipal_RoundTripsReadme()
  {
    var data = new ProcessorData
    {
      Id = Guid.NewGuid(),
      UserId = "user-123",
      Name = "processor",
      Readme = "readme-content",
    };
    var principal = data.ToPrincipal();
    principal.Metadata.Readme.Should().Be("readme-content");
  }

  [Fact]
  public void ResolverData_ToPrincipal_RoundTripsReadme()
  {
    var data = new ResolverData
    {
      Id = Guid.NewGuid(),
      UserId = "user-123",
      Name = "resolver",
      Readme = "readme-content",
    };
    var principal = data.ToPrincipal();
    principal.Metadata.Readme.Should().Be("readme-content");
  }
}
