using System.Net;
using BaseMicroservice;
using DatabaseService.Models.Rabbit;
using FluentAssertions;
using OpenTelemetry.Trace;

namespace UnitTests.BaseMicroservice;

public class UserCredentialsService_Tests
{
    private const string BaseUrl = "https://mocked.api";

    [TestCase("testuser1", ChannelType.Email)]
    [TestCase("testuser2", ChannelType.Telegram)]
    public async Task Should_calls_http_client_with_correct_request_url(string userName, ChannelType channelType)
    {
        var expectedRequestUrl = $"/user/getCredentials/{userName}/{channelType}";

        var handler = new FakeHttpMessageHandler(request =>
        {
            request.Method.Should().Be(HttpMethod.Get);
            request.RequestUri.Should().Be(BaseUrl + expectedRequestUrl);

            return new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"credentials\": \"mocked-credential\"}")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };

        var service = new UserCredentialsService(httpClient);

        await service.GetCredentials(userName, channelType);
    }

   [TestCaseSource(nameof(TestDataForCheckReturnCorrectlyCredentials))]
    public async Task Should_return_correctly_credentials_depending_on_result_of_request((HttpStatusCode statusCode, StringContent content, string? expectedResult) td)
    {
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage
            {
                StatusCode = td.statusCode,
                Content = td.content
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(BaseUrl)
        };
        
        var service = new UserCredentialsService(httpClient);

        var result = await service.GetCredentials("testuser", ChannelType.Email);

        result.Should().Be(td.expectedResult);
    }

    private static IEnumerable<(HttpStatusCode, StringContent, string?)> TestDataForCheckReturnCorrectlyCredentials()
    {
        yield return (
            HttpStatusCode.NotFound, 
            new StringContent("{\"credentials\": \"expectedCredentials\"}"), 
            null);
        yield return (
            HttpStatusCode.OK, 
            new StringContent("{\"credentials\": \"expectedCredentials\"}"), 
            "expectedCredentials");
        yield return (
            HttpStatusCode.NotFound, 
            null, 
            null);
    }
}