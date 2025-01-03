namespace UnitTests.BaseMicroservice;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handlerFunc;

    public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handlerFunc)
    {
        _handlerFunc = handlerFunc ?? throw new ArgumentNullException(nameof(handlerFunc));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_handlerFunc(request));
    }
}