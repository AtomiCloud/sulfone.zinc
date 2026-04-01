using CSharp_Result;
using Domain.Model;
using Domain.Repository;
using Domain.Service;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace UnitTest;

/// <summary>
/// Service-level tests proving that PluginService.Push() routes through the
/// atomic UpdateAndCreateVersion path for existing entities, passes updated
/// metadata (including readme), and propagates failures.
/// </summary>
public class PluginServicePushTests
{
  private readonly IPluginRepository _repo;
  private readonly IUserRepository _userRepo;
  private readonly ILogger<PluginService> _logger;
  private readonly PluginService _service;

  public PluginServicePushTests()
  {
    _repo = Substitute.For<IPluginRepository>();
    _userRepo = Substitute.For<IUserRepository>();
    _logger = Substitute.For<ILogger<PluginService>>();
    _service = new PluginService(_repo, _logger, _userRepo);
  }

  private static PluginMetadata NewMetadata(string readme) => new()
  {
    Project = "proj",
    Source = "src",
    Email = "e@e.com",
    Tags = ["tag"],
    Description = "desc",
    Readme = readme,
  };

  private static Plugin ExistingPlugin() => new()
  {
    Principal = new PluginPrincipal
    {
      Id = Guid.NewGuid(),
      UserId = "user-id",
      Metadata = NewMetadata("original-readme"),
      Record = new PluginRecord { Name = "test-plugin" },
    },
    User = new UserPrincipal
    {
      Id = "user-id",
      Record = new UserRecord { Username = "testuser" },
    },
    Versions = [],
    Info = new PluginInfo { Downloads = 0, Dependencies = 0, Stars = 0 },
  };

  private static PluginVersionPrincipal NewVersionResult(ulong version = 1) => new()
  {
    Id = Guid.NewGuid(),
    Version = version,
    CreatedAt = DateTime.UtcNow,
    Record = new PluginVersionRecord { Description = $"v{version}" },
    Property = new PluginVersionProperty
    {
      DockerReference = "docker/ref",
      DockerTag = "latest",
    },
  };

  private static readonly PluginRecord PRecord = new() { Name = "test-plugin" };
  private static readonly PluginVersionRecord VRecord = new() { Description = "v1" };
  private static readonly PluginVersionProperty VProp = new()
  {
    DockerReference = "docker/ref",
    DockerTag = "latest",
  };

  /// <summary>
  /// Verifies that pushing an existing plugin calls the atomic
  /// UpdateAndCreateVersion method, NOT the separate Create + CreateVersion.
  /// </summary>
  [Fact]
  public async Task Push_ExistingPlugin_CallsUpdateAndCreateVersion()
  {
    // Arrange
    var plugin = ExistingPlugin();
    var version = NewVersionResult();
    var metadata = NewMetadata("updated-readme");

    _repo.Get("testuser", "test-plugin")
      .Returns(Task.FromResult<Result<Plugin?>>(plugin));
    _repo.UpdateAndCreateVersion("testuser", "test-plugin", metadata, VRecord, VProp)
      .Returns(Task.FromResult<Result<PluginVersionPrincipal?>>(version));

    // Act
    var result = await _service.Push("testuser", PRecord, metadata, VRecord, VProp);

    // Assert
    result.IsSuccess().Should().BeTrue();
    await _repo.Received(1).UpdateAndCreateVersion(
      "testuser", "test-plugin", metadata, VRecord, VProp);
    await _repo.DidNotReceive().Create(
      Arg.Any<string>(), Arg.Any<PluginRecord>(), Arg.Any<PluginMetadata>());
    await _repo.DidNotReceive().CreateVersion(
      Arg.Any<string>(), Arg.Any<string>(),
      Arg.Any<PluginVersionRecord>(), Arg.Any<PluginVersionProperty>());
  }

  /// <summary>
  /// Verifies that repeated pushes of an existing plugin pass the latest
  /// readme value through to UpdateAndCreateVersion. This is the core
  /// regression test for stale top-level metadata.
  /// </summary>
  [Fact]
  public async Task Push_ExistingPlugin_RepeatedPush_PassesLatestReadme()
  {
    // Arrange
    var plugin = ExistingPlugin();
    var metadata1 = NewMetadata("readme-v1");
    var metadata2 = NewMetadata("readme-v2");

    _repo.Get("testuser", "test-plugin")
      .Returns(Task.FromResult<Result<Plugin?>>(plugin));
    _repo.UpdateAndCreateVersion(
        Arg.Any<string>(), Arg.Any<string>(),
        Arg.Any<PluginMetadata>(),
        Arg.Any<PluginVersionRecord>(), Arg.Any<PluginVersionProperty>())
      .Returns(Task.FromResult<Result<PluginVersionPrincipal?>>(NewVersionResult()));

    // Act — first push
    var result1 = await _service.Push("testuser", PRecord, metadata1, VRecord, VProp);
    result1.IsSuccess().Should().BeTrue();

    // Act — second push with updated readme
    var result2 = await _service.Push("testuser", PRecord, metadata2, VRecord, VProp);

    // Assert — both pushes went through UpdateAndCreateVersion
    result2.IsSuccess().Should().BeTrue();
    await _repo.Received(2).UpdateAndCreateVersion(
      "testuser", "test-plugin",
      Arg.Any<PluginMetadata>(),
      VRecord, VProp);

    // Assert — the second push passed metadata with the latest readme
    await _repo.Received(1).UpdateAndCreateVersion(
      "testuser", "test-plugin",
      Arg.Is<PluginMetadata>(m => m.Readme == "readme-v2"),
      VRecord, VProp);
  }

  /// <summary>
  /// Verifies that when UpdateAndCreateVersion fails, the failure propagates
  /// through the service and no fallback CreateVersion call is made.
  /// This proves the atomicity contract: metadata update + version creation
  /// are a single all-or-nothing operation at the repository layer.
  /// </summary>
  [Fact]
  public async Task Push_ExistingPlugin_UpdateAndCreateVersionFails_PropagatesFailure()
  {
    // Arrange
    var plugin = ExistingPlugin();
    var metadata = NewMetadata("updated-readme");
    var error = new Exception("Version creation failed");

    _repo.Get("testuser", "test-plugin")
      .Returns(Task.FromResult<Result<Plugin?>>(plugin));
    _repo.UpdateAndCreateVersion(
        "testuser", "test-plugin", metadata, VRecord, VProp)
      .Returns(Task.FromResult<Result<PluginVersionPrincipal?>>(error));

    // Act
    var result = await _service.Push("testuser", PRecord, metadata, VRecord, VProp);

    // Assert — failure propagates, no fallback CreateVersion
    result.IsFailure().Should().BeTrue();
    await _repo.DidNotReceive().CreateVersion(
      Arg.Any<string>(), Arg.Any<string>(),
      Arg.Any<PluginVersionRecord>(), Arg.Any<PluginVersionProperty>());
  }

  /// <summary>
  /// Verifies that pushing a NEW plugin (one that doesn't exist yet) takes
  /// the Create + CreateVersion path, not UpdateAndCreateVersion.
  /// </summary>
  [Fact]
  public async Task Push_NewPlugin_CallsCreateAndCreateVersion()
  {
    // Arrange
    var user = new User
    {
      Principal = new UserPrincipal
      {
        Id = "user-id",
        Record = new UserRecord { Username = "testuser" },
      },
      Tokens = [],
    };
    var createdPlugin = new PluginPrincipal
    {
      Id = Guid.NewGuid(),
      UserId = "user-id",
      Metadata = NewMetadata("initial-readme"),
      Record = new PluginRecord { Name = "test-plugin" },
    };
    var version = NewVersionResult();
    var metadata = NewMetadata("initial-readme");

    _repo.Get("testuser", "test-plugin")
      .Returns(Task.FromResult<Result<Plugin?>>((Plugin?)null));
    _userRepo.GetByUsername("testuser")
      .Returns(Task.FromResult<Result<User?>>(user));
    _repo.Create("user-id", PRecord, metadata)
      .Returns(Task.FromResult<Result<PluginPrincipal>>(createdPlugin));
    _repo.CreateVersion("testuser", "test-plugin", VRecord, VProp)
      .Returns(Task.FromResult<Result<PluginVersionPrincipal?>>(version));

    // Act
    var result = await _service.Push("testuser", PRecord, metadata, VRecord, VProp);

    // Assert
    result.IsSuccess().Should().BeTrue();
    await _repo.Received(1).Create("user-id", PRecord, metadata);
    await _repo.Received(1).CreateVersion("testuser", "test-plugin", VRecord, VProp);
    await _repo.DidNotReceive().UpdateAndCreateVersion(
      Arg.Any<string>(), Arg.Any<string>(),
      Arg.Any<PluginMetadata>(), Arg.Any<PluginVersionRecord>(),
      Arg.Any<PluginVersionProperty>());
  }
}
