using System.Net;

namespace UnitTests.TelegramNotifier;

public class MockHttpMessageHandler : HttpMessageHandler
{
    public List<HttpRequestMessage> Requests { get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}