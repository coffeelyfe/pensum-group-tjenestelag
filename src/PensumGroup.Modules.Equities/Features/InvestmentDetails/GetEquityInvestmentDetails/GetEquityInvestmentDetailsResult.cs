using System.Text.Json;

namespace PensumGroup.Modules.Equities.Features.InvestmentDetails.GetEquityInvestmentDetails;

public enum GetEquityInvestmentDetailsResultStatus
{
    Success,
    NotFound,
    Failure,
}

public sealed class GetEquityInvestmentDetailsResult
{
    private GetEquityInvestmentDetailsResult(GetEquityInvestmentDetailsResultStatus status, JsonElement? value, string? error)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public GetEquityInvestmentDetailsResultStatus Status { get; }

    public JsonElement? Value { get; }

    public string? Error { get; }

    public static GetEquityInvestmentDetailsResult Success(JsonElement value) =>
        new(GetEquityInvestmentDetailsResultStatus.Success, value, null);

    public static GetEquityInvestmentDetailsResult NotFound() =>
        new(GetEquityInvestmentDetailsResultStatus.NotFound, null, null);

    public static GetEquityInvestmentDetailsResult Failure(string error) =>
        new(GetEquityInvestmentDetailsResultStatus.Failure, null, error);
}
