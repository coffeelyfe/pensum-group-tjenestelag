using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using PensumGroup.Modules.Equities.Integrations.Morningstar;
using Xunit;

namespace PensumGroup.Modules.Equities.Tests.Integrations.Morningstar;

public sealed class MorningstarClientTests
{
    [Theory]
    [InlineData("", "user", "pass")]
    [InlineData("https://morningstar.example.com", "", "pass")]
    [InlineData("https://morningstar.example.com", "user", "")]
    [InlineData("not-a-valid-url", "user", "pass")]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsFailure_WhenConfigurationIsInvalid(
        string baseUrl, string username, string password)
    {
        var handler = new FakeHttpMessageHandler();
        var client = CreateClient(handler, new MorningstarOptions
        {
            BaseUrl = baseUrl,
            Username = username,
            Password = password,
        });

        var result = await client.GetEquityInvestmentDetailsAsync("AAPL", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.Failure, result.Status);
        Assert.NotNull(result.Error);
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsSuccess_WhenMorningstarRespondsWithData()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, """{"access_token":"test-token"}"""));
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, """{"ticker":"AAPL","name":"Apple Inc"}"""));

        var client = CreateClient(handler, ValidOptions());

        var result = await client.GetEquityInvestmentDetailsAsync("AAPL", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.Success, result.Status);
        Assert.Equal("AAPL", result.Value.GetProperty("ticker").GetString());
        Assert.Equal("Bearer test-token", handler.Requests[1].Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_EscapesIdentifierInRequestUri()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, """{"access_token":"test-token"}"""));
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, "{}"));

        var client = CreateClient(handler, ValidOptions());

        await client.GetEquityInvestmentDetailsAsync("AAPL US", CancellationToken.None);

        Assert.Contains("AAPL%20US", handler.Requests[1].RequestUri!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsNotFound_WhenDetailsResponseIs404()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, """{"access_token":"test-token"}"""));
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        var client = CreateClient(handler, ValidOptions());

        var result = await client.GetEquityInvestmentDetailsAsync("UNKNOWN", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsNotFound_WhenDetailsResponseIsEmptyArray()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, """{"access_token":"test-token"}"""));
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, "[]"));

        var client = CreateClient(handler, ValidOptions());

        var result = await client.GetEquityInvestmentDetailsAsync("UNKNOWN", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsFailure_WhenTokenEndpointFails()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized));

        var client = CreateClient(handler, ValidOptions());

        var result = await client.GetEquityInvestmentDetailsAsync("AAPL", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.Failure, result.Status);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsFailure_WhenTokenResponseIsNotJson()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json", Encoding.UTF8, "text/plain"),
        });

        var client = CreateClient(handler, ValidOptions());

        var result = await client.GetEquityInvestmentDetailsAsync("AAPL", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.Failure, result.Status);
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsFailure_WhenDetailsResponseIsNotJson()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, """{"access_token":"test-token"}"""));
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not json", Encoding.UTF8, "text/plain"),
        });

        var client = CreateClient(handler, ValidOptions());

        var result = await client.GetEquityInvestmentDetailsAsync("AAPL", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.Failure, result.Status);
    }

    [Fact]
    public async Task GetEquityInvestmentDetailsAsync_ReturnsFailure_WhenDetailsEndpointReturnsServerError()
    {
        var handler = new FakeHttpMessageHandler();
        handler.Enqueue(_ => JsonResponse(HttpStatusCode.OK, """{"access_token":"test-token"}"""));
        handler.Enqueue(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var client = CreateClient(handler, ValidOptions());

        var result = await client.GetEquityInvestmentDetailsAsync("AAPL", CancellationToken.None);

        Assert.Equal(MorningstarResultStatus.Failure, result.Status);
    }

    private static MorningstarOptions ValidOptions() => new()
    {
        BaseUrl = "https://morningstar.example.com",
        Username = "user",
        Password = "pass",
    };

    private static MorningstarClient CreateClient(HttpMessageHandler handler, MorningstarOptions options) =>
        new(new HttpClient(handler), Options.Create(options));

    private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) => new(statusCode)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json"),
    };
}
