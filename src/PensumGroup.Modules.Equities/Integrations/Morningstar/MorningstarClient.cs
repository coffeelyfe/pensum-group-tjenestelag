using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace PensumGroup.Modules.Equities.Integrations.Morningstar;

public sealed class MorningstarClient(HttpClient httpClient, IOptions<MorningstarOptions> options) : IMorningstarClient
{
    public async Task<MorningstarResult<JsonElement>> GetEquityInvestmentDetailsAsync(string identifier, CancellationToken cancellationToken)
    {
        var morningstarOptions = options.Value;

        if (string.IsNullOrWhiteSpace(morningstarOptions.BaseUrl)
            || string.IsNullOrWhiteSpace(morningstarOptions.Username)
            || string.IsNullOrWhiteSpace(morningstarOptions.Password)
            || !Uri.TryCreate(morningstarOptions.BaseUrl, UriKind.Absolute, out _))
        {
            return MorningstarResult<JsonElement>.Failure("Morningstar configuration is missing or invalid.");
        }

        string accessToken;
        try
        {
            var token = await RequestAccessTokenAsync(morningstarOptions, cancellationToken);
            if (string.IsNullOrEmpty(token))
            {
                return MorningstarResult<JsonElement>.Failure("Morningstar did not return an access token.");
            }

            accessToken = token;
        }
        catch (HttpRequestException ex)
        {
            return MorningstarResult<JsonElement>.Failure($"Failed to authenticate with Morningstar: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return MorningstarResult<JsonElement>.Failure($"Failed to parse Morningstar token response: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return MorningstarResult<JsonElement>.Failure($"Morningstar authentication request timed out: {ex.Message}");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildDetailsUri(morningstarOptions, identifier));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var response = await httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return MorningstarResult<JsonElement>.NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                return MorningstarResult<JsonElement>.Failure(
                    $"Morningstar responded with status code {(int)response.StatusCode}.");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
            {
                return MorningstarResult<JsonElement>.NotFound();
            }

            using var document = JsonDocument.Parse(content);
            var root = document.RootElement.Clone();

            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() == 0)
            {
                return MorningstarResult<JsonElement>.NotFound();
            }

            return MorningstarResult<JsonElement>.Success(root);
        }
        catch (HttpRequestException ex)
        {
            return MorningstarResult<JsonElement>.Failure($"Failed to call Morningstar: {ex.Message}");
        }
        catch (JsonException ex)
        {
            return MorningstarResult<JsonElement>.Failure($"Failed to parse Morningstar response: {ex.Message}");
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            return MorningstarResult<JsonElement>.Failure($"Morningstar request timed out: {ex.Message}");
        }
    }

    private async Task<string?> RequestAccessTokenAsync(MorningstarOptions morningstarOptions, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildTokenUri(morningstarOptions))
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["userid"] = morningstarOptions.Username,
                ["password"] = morningstarOptions.Password,
            }),
        };

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Morningstar token endpoint responded with status code {(int)response.StatusCode}.");
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var tokenResponse = JsonSerializer.Deserialize<MorningstarTokenResponse>(content);

        return tokenResponse?.AccessToken;
    }

    private static Uri BuildTokenUri(MorningstarOptions options) =>
        new(new Uri(options.BaseUrl, UriKind.Absolute), options.TokenPath);

    private static Uri BuildDetailsUri(MorningstarOptions options, string identifier)
    {
        var path = options.EquityInvestmentDetailsPathTemplate.Replace(
            "{identifier}", Uri.EscapeDataString(identifier), StringComparison.Ordinal);

        return new Uri(new Uri(options.BaseUrl, UriKind.Absolute), path);
    }
}
