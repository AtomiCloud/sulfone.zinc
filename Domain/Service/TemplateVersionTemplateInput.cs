using System.Text.Json;
using Domain.Model;

namespace Domain.Service;

/// <summary>
/// Service-layer input type for passing sub-template data from API to service.
/// Contains the unresolved reference and associated preset answer configurations.
/// </summary>
/// <param name="Template">Unresolved sub-template reference (Username, Name, Version?) used for lookup</param>
/// <param name="PresetAnswers">Dynamic JSON preset answer configuration for the sub-template</param>
public record TemplateVersionTemplateInput(TemplateVersionRef Template, JsonElement PresetAnswers);
