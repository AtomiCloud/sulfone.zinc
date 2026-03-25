using System.Text.Json;

namespace Domain.Model;

/// <summary>
/// Represents a template's reference to a sub-template version with associated preset answer configurations.
/// This is the domain model used when reading template-template relationships.
/// </summary>
/// <param name="Template">The resolved sub-template version principal</param>
/// <param name="PresetAnswers">Dynamic JSON preset answer configuration for the sub-template (strings, arrays, bools, or nested objects)</param>
public record TemplateVersionTemplateRef(
  TemplateVersionPrincipal Template,
  JsonElement PresetAnswers
);
