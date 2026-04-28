using AspireApp.Api;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// EF Core InMemory — no Docker required
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("AspireApp"));

builder.Services.AddAuthorization();

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Items.AddRange(
        new Item { Id = 1, Name = "Item One" },
        new Item { Id = 2, Name = "Item Two" });
    db.SaveChanges();
}

app.MapDefaultEndpoints();
app.UseAuthorization();

app.MapGet("/api/items", async (AppDbContext db) =>
{
    var items = await db.Items.ToListAsync();
    return Results.Ok(items.Select(i => new { id = i.Id, name = i.Name }));
});

app.MapGet("/api/profile", (HttpContext context) =>
{
    if (context.User.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    var email = context.User.FindFirstValue(ClaimTypes.Email) ?? "unknown";
    var displayName = context.User.FindFirstValue(ClaimTypes.Name) ?? email;

    return Results.Ok(new { email, displayName });
});

app.Run();
