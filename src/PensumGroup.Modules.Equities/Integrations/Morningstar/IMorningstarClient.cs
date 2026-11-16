using System.Text.Json;

namespace PensumGroup.Modules.Equities.Integrations.Morningstar;

public interface IMorningstarClient
{
    Task<MorningstarResult<JsonElement>> GetEquityInvestmentDetailsAsync(string identifier, CancellationToken cancellationToken);
}
