using Blazored.LocalStorage;
using System.Net.Http.Headers;

public class HttpClientHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public HttpClientHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrEmpty(token))
        {
            Console.WriteLine($"Adding token to request header: {token}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken); // Ensure the base handler is properly called
    }
}
