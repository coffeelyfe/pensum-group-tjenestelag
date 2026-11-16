using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PensumGroup.Modules.Equities.Features.InvestmentDetails.GetEquityInvestmentDetails;

namespace PensumGroup.API.Controllers;

[ApiController]
[Authorize]
[Route("api/equities")]
public sealed class EquitiesController(IGetEquityInvestmentDetailsFeature getEquityInvestmentDetailsFeature) : ControllerBase
{
    [HttpGet("{identifier}/details")]
    public async Task<IActionResult> GetDetails(string identifier, CancellationToken cancellationToken)
    {
        var result = await getEquityInvestmentDetailsFeature.ExecuteAsync(identifier, cancellationToken);

        return result.Status switch
        {
            GetEquityInvestmentDetailsResultStatus.Success => Ok(result.Value),
            GetEquityInvestmentDetailsResultStatus.NotFound => Problem(
                detail: $"No equity investment details found for identifier '{identifier}'.",
                statusCode: StatusCodes.Status404NotFound),
            _ => Problem(
                detail: result.Error ?? "Failed to retrieve equity investment details from Morningstar.",
                statusCode: StatusCodes.Status502BadGateway),
        };
    }
}
