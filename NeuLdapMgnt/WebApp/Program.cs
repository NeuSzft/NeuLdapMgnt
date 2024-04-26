using BlazorBootstrap;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using NeuLdapMgnt.WebApp;
using NeuLdapMgnt.WebApp.Requests;
using NeuLdapMgnt.WebApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazorBootstrap();

ApiRequests apiRequests;
if (builder.HostEnvironment.IsDevelopment())
{
	apiRequests = new("http://localhost:5000");
	builder.Services.AddSingleton<ApiRequests>(_ => apiRequests);
}
else
{
	apiRequests = new(builder.HostEnvironment.BaseAddress);
	builder.Services.AddSingleton<ApiRequests>(_ => apiRequests);
}

ToastService toastService = new();
ModalService modalService = new();

builder.Services.AddSingleton<ToastService>(_ => toastService);
builder.Services.AddSingleton<ModalService>(_ => modalService);
builder.Services.AddSingleton<LocalDbService>(_ => new(apiRequests, toastService));

await builder.Build().RunAsync();
