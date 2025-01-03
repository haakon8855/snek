using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Web.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddHttpClient("ServerApi", c =>
{
    c.BaseAddress = builder.Configuration.GetSection("Endpoints:ServerApi").Get<Uri>();
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var jsRuntime = services.GetRequiredService<IJSRuntime>();
    await jsRuntime.InvokeVoidAsync("JsFunctions.addKeyboardListenerEvent");
}

await app.RunAsync();

