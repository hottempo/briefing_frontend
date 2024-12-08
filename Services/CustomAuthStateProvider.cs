using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Check if this is pre-rendering
        var isPreRendering = !OperatingSystem.IsBrowser();

        // If pre-rendering, return an anonymous user
        if (isPreRendering)
        {
            return new AuthenticationState(_anonymous);
        }

        // After the initial pre-rendering, get the token
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(_anonymous);
        }

        var identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public void NotifyUserAuthentication(string token)
    {
        var identity = new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

        Console.WriteLine("User authenticated with token: " + token);
    }

    public void NotifyUserLogout()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));

        Console.WriteLine("User logged out");
    }
}

public static class JwtParser
{
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = Convert.FromBase64String(FixBase64ForUrl(payload));
        var keyValuePairs = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));

        return claims;
    }

    private static string FixBase64ForUrl(string base64)
    {
        base64 = base64.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return base64;
    }
}
