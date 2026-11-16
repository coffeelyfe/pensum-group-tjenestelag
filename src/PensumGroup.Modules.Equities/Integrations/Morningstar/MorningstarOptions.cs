namespace PensumGroup.Modules.Equities.Integrations.Morningstar;

public sealed class MorningstarOptions
{
    public const string SectionName = "Morningstar";

    public string BaseUrl { get; init; } = string.Empty;

    public string TokenPath { get; init; } = "/token/oauth";

    public string EquityInvestmentDetailsPathTemplate { get; init; } = "/v2/investment-details/equities/{identifier}";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
