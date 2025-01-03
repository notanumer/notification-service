using System.Net;
using DatabaseService.Models.Rabbit;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BaseMicroservice;

public interface IUserCredentialsService
{
    Task<string?> GetCredentials(string userName, ChannelType channelType);
}

public class UserCredentialsService : IUserCredentialsService
{
    HttpClient client;

    public UserCredentialsService(string credentialsApiUri)
    {
        client = new HttpClient() { BaseAddress = new Uri(credentialsApiUri) };
    }

    public UserCredentialsService(HttpClient client)
    {
        this.client = client;
    }

    public async Task<string?> GetCredentials(string userName, ChannelType channelType)
    {
        var resp = await client.GetAsync($"/user/getCredentials/{userName}/{channelType}");

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return null;

        var result = JsonConvert.DeserializeObject<JToken>(await resp.Content.ReadAsStringAsync());
        return result?["credentials"].ToString();
    }
}