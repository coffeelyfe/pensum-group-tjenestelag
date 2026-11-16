using PensumGroup.Modules.Equities.Integrations.Morningstar;

namespace PensumGroup.Modules.Equities.Features.InvestmentDetails.GetEquityInvestmentDetails;

public sealed class GetEquityInvestmentDetailsFeature(IMorningstarClient morningstarClient) : IGetEquityInvestmentDetailsFeature
{
    public async Task<GetEquityInvestmentDetailsResult> ExecuteAsync(string identifier, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return GetEquityInvestmentDetailsResult.Failure("Identifier must not be empty.");
        }

        var result = await morningstarClient.GetEquityInvestmentDetailsAsync(identifier, cancellationToken);

        return result.Status switch
        {
            MorningstarResultStatus.Success => GetEquityInvestmentDetailsResult.Success(result.Value!),
            MorningstarResultStatus.NotFound => GetEquityInvestmentDetailsResult.NotFound(),
            _ => GetEquityInvestmentDetailsResult.Failure(result.Error ?? "Morningstar request failed."),
        };
    }
}
