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

JwtService jwtService = new();
builder.Services.AddSingleton<JwtService>(_ => jwtService);

if (builder.HostEnvironment.IsDevelopment())
{
	apiRequests = new("http://localhost:5000", jwtService);
	builder.Services.AddSingleton<ApiRequests>(_ => apiRequests);
}
else
{
	apiRequests = new(builder.HostEnvironment.BaseAddress, jwtService);
	builder.Services.AddSingleton<ApiRequests>(_ => apiRequests);
}

ToastService        toastService        = new();
ModalService        modalService        = new();
NotificationService notificationService = new(toastService);
LocalDbService      localDbService      = new(apiRequests, notificationService);

builder.Services.AddSingleton<ToastService>(_ => toastService);
builder.Services.AddSingleton<ModalService>(_ => modalService);
builder.Services.AddSingleton<LocalDbService>(_ => localDbService);
builder.Services.AddSingleton<NotificationService>(_ => notificationService);
builder.Services.AddSingleton<StudentService>(_ => new(apiRequests, localDbService, notificationService));
builder.Services.AddSingleton<TeacherService>(_ => new(apiRequests, localDbService, notificationService));

await builder.Build().RunAsync();
