using System.Net.Http.Headers;

namespace Cli.Helpers;

public class HttpClientWrapper
{
    public HttpClient httpClient { get; set; }
    public HttpClientWrapper(string jwt) {
        httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", jwt
        );
    }
}