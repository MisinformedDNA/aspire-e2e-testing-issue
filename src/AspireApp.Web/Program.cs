using Clerk.Blazor;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddClerkAuth();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapDefaultEndpoints();
app.MapRazorComponents<AspireApp.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
