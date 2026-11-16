using System.Text.Json.Serialization;

namespace PensumGroup.Modules.Equities.Integrations.Morningstar;

internal sealed class MorningstarTokenResponse
{
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }
}
