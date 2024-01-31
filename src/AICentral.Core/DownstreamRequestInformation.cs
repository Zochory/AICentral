namespace AICentral.Core;

/// <summary>
/// Represents the request to the Downstream AI Service before metadata is extracted
/// </summary>
/// <param name="LanguageUrl">Url called</param>
/// <param name="CallType">Type of AI Call</param>
/// <param name="DeploymentName">Which Deployment (or Model) was called</param>
/// <param name="Prompt">The prompt passed by the consumer</param>
/// <param name="StartDate">When the call to the downstream was made</param>
/// <param name="Duration">Duration of the downstream call</param>
public record DownstreamRequestInformation(string LanguageUrl, AICallType CallType, string? DeploymentName, string? Prompt, DateTimeOffset StartDate, TimeSpan Duration);
