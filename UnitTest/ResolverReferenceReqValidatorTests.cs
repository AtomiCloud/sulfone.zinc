using System.Text.Json;
using App.Modules.Cyan.API.V1.Models;
using App.Modules.Cyan.API.V1.Validators;
using FluentValidation;
using FluentValidation.TestHelper;

namespace UnitTest;

public class ResolverReferenceReqValidatorTests
{
  private readonly ResolverReferenceReqValidator _validator = new();

  [Fact]
  public void Validator_WithValidData_PassesValidation()
  {
    // Arrange
    var configJson = JsonDocument.Parse("""{"strategy": "deep-merge"}""").RootElement;
    var req = new ResolverReferenceReq(
      "atomi",
      "json-merger",
      1,
      configJson,
      new[] { "package.json" }
    );

    // Act
    var result = _validator.TestValidate(req);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Validator_WithEmptyFiles_PassesValidation()
  {
    // Arrange - Files can be empty array, just not null
    var configJson = JsonDocument.Parse("{}").RootElement;
    var req = new ResolverReferenceReq(
      "atomi",
      "json-merger",
      1,
      configJson,
      Array.Empty<string>()
    );

    // Act
    var result = _validator.TestValidate(req);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }

  [Fact]
  public void Validator_WithNullFiles_FailsValidation()
  {
    // Arrange
    var configJson = JsonDocument.Parse("{}").RootElement;
    var req = new ResolverReferenceReq("atomi", "json-merger", 1, configJson, null!);

    // Act
    var result = _validator.TestValidate(req);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Files);
  }

  [Fact]
  public void Validator_WithNullUsername_FailsValidation()
  {
    // Arrange
    var configJson = JsonDocument.Parse("{}").RootElement;
    var req = new ResolverReferenceReq(null!, "json-merger", 1, configJson, Array.Empty<string>());

    // Act
    var result = _validator.TestValidate(req);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Username);
  }

  [Fact]
  public void Validator_WithNullName_FailsValidation()
  {
    // Arrange
    var configJson = JsonDocument.Parse("{}").RootElement;
    var req = new ResolverReferenceReq("atomi", null!, 1, configJson, Array.Empty<string>());

    // Act
    var result = _validator.TestValidate(req);

    // Assert
    result.ShouldHaveValidationErrorFor(x => x.Name);
  }

  [Fact]
  public void Validator_WithMultipleGlobPatterns_PassesValidation()
  {
    // Arrange
    var configJson = JsonDocument.Parse("{}").RootElement;
    var req = new ResolverReferenceReq(
      "atomi",
      "json-merger",
      1,
      configJson,
      new[] { "package.json", "**/tsconfig.json", "src/**/*.yaml" }
    );

    // Act
    var result = _validator.TestValidate(req);

    // Assert
    result.ShouldNotHaveAnyValidationErrors();
  }
}
