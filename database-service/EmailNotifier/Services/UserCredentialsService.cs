using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EmailNotifier.Services;

public class UserCredentialsService
{
    HttpClient client;

    public UserCredentialsService(string credentialsApiUri)
    {
        client = new HttpClient() { BaseAddress = new Uri(credentialsApiUri) };
    }

    public async Task<string?> GetCredentials(string userName)
    {
        var resp = await client.GetAsync($"/user/getCredentials/{userName}/Email");

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        var result = JsonConvert.DeserializeObject<JToken>(await resp.Content.ReadAsStringAsync());
        return result?["credentials"].ToString();
    }
}