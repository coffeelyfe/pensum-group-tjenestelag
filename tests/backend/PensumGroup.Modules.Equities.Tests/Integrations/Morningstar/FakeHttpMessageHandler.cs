namespace PensumGroup.Modules.Equities.Tests.Integrations.Morningstar;

internal sealed class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

    public List<HttpRequestMessage> Requests { get; } = [];

    public void Enqueue(Func<HttpRequestMessage, HttpResponseMessage> responseFactory) => _responses.Enqueue(responseFactory);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No more responses queued for FakeHttpMessageHandler.");
        }

        return Task.FromResult(_responses.Dequeue()(request));
    }
}
