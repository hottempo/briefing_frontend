using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Net.Http.Headers;
using briefing_frontend.Components;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Blazored LocalStorage to store the JWT token
builder.Services.AddBlazoredLocalStorage();

// Add the Custom Authentication State Provider to handle JWT tokens
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

builder.Services.AddScoped<DelegatingHandler, HttpClientHandler>(); // Register custom handler as DelegatingHandler
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<DelegatingHandler>(); // Get the custom handler
    var client = new HttpClient(handler)
    {
        BaseAddress = new Uri("http://192.168.1.244:50002/") // BFF URL
    };
    return client;
});


var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
