using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NeuLdapMgnt.WebApp;
using NeuLdapMgnt.WebApp.Requests;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorBootstrap();
builder.Services.AddSingleton<Utils>();
builder.Services.AddSingleton<ApiRequests>(_ => new(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
