namespace PensumGroup.Modules.Equities.Features.InvestmentDetails.GetEquityInvestmentDetails;

public interface IGetEquityInvestmentDetailsFeature
{
    Task<GetEquityInvestmentDetailsResult> ExecuteAsync(string identifier, CancellationToken cancellationToken);
}
