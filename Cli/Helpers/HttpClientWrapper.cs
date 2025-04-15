using System.Net.Http.Headers;

namespace Cli.Helpers;

public class HttpClientWrapper
{
    private HttpClient _httpClient { get; set; }
    public HttpClientWrapper(string jwt) {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer ", jwt
        );
    }
}