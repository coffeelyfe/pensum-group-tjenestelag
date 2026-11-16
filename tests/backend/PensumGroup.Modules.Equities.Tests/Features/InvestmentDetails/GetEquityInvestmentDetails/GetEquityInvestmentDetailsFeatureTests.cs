using System.Text.Json;
using PensumGroup.Modules.Equities.Features.InvestmentDetails.GetEquityInvestmentDetails;
using PensumGroup.Modules.Equities.Integrations.Morningstar;
using Xunit;

namespace PensumGroup.Modules.Equities.Tests.Features.InvestmentDetails.GetEquityInvestmentDetails;

public sealed class GetEquityInvestmentDetailsFeatureTests
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSuccess_WhenMorningstarClientSucceeds()
    {
        using var document = JsonDocument.Parse("""{"ticker":"AAPL"}""");
        var fakeClient = new FakeMorningstarClient(MorningstarResult<JsonElement>.Success(document.RootElement.Clone()));
        var feature = new GetEquityInvestmentDetailsFeature(fakeClient);

        var result = await feature.ExecuteAsync("AAPL", CancellationToken.None);

        Assert.Equal(GetEquityInvestmentDetailsResultStatus.Success, result.Status);
        Assert.Equal("AAPL", result.Value!.Value.GetProperty("ticker").GetString());
        Assert.Equal("AAPL", fakeClient.ReceivedIdentifier);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotFound_WhenMorningstarClientReturnsNotFound()
    {
        var fakeClient = new FakeMorningstarClient(MorningstarResult<JsonElement>.NotFound());
        var feature = new GetEquityInvestmentDetailsFeature(fakeClient);

        var result = await feature.ExecuteAsync("UNKNOWN", CancellationToken.None);

        Assert.Equal(GetEquityInvestmentDetailsResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsFailure_WhenMorningstarClientFails()
    {
        var fakeClient = new FakeMorningstarClient(MorningstarResult<JsonElement>.Failure("boom"));
        var feature = new GetEquityInvestmentDetailsFeature(fakeClient);

        var result = await feature.ExecuteAsync("AAPL", CancellationToken.None);

        Assert.Equal(GetEquityInvestmentDetailsResultStatus.Failure, result.Status);
        Assert.Equal("boom", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_ReturnsFailure_WhenIdentifierIsBlank(string identifier)
    {
        var fakeClient = new FakeMorningstarClient(MorningstarResult<JsonElement>.Failure("should not be called"));
        var feature = new GetEquityInvestmentDetailsFeature(fakeClient);

        var result = await feature.ExecuteAsync(identifier, CancellationToken.None);

        Assert.Equal(GetEquityInvestmentDetailsResultStatus.Failure, result.Status);
        Assert.Null(fakeClient.ReceivedIdentifier);
    }
}
