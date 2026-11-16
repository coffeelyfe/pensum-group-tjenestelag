using System.Text.Json;
using PensumGroup.Modules.Equities.Integrations.Morningstar;

namespace PensumGroup.Modules.Equities.Tests.Features.InvestmentDetails.GetEquityInvestmentDetails;

internal sealed class FakeMorningstarClient : IMorningstarClient
{
    private readonly MorningstarResult<JsonElement> _result;

    public FakeMorningstarClient(MorningstarResult<JsonElement> result) => _result = result;

    public string? ReceivedIdentifier { get; private set; }

    public Task<MorningstarResult<JsonElement>> GetEquityInvestmentDetailsAsync(string identifier, CancellationToken cancellationToken)
    {
        ReceivedIdentifier = identifier;
        return Task.FromResult(_result);
    }
}
