using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Azure Storage Queues
builder.AddAzureQueueClient("queues");

// CosmosDB
builder.AddAzureCosmosClient("cosmosdb");

// JWT Bearer auth via Clerk
var clerkAuthority = builder.Configuration["Clerk:Authority"];
if (!string.IsNullOrEmpty(clerkAuthority))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = clerkAuthority;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                NameClaimType = ClaimTypes.Email,
            };
        });
}
else
{
    builder.Services.AddAuthentication();
}

builder.Services.AddAuthorization();

var app = builder.Build();

app.MapDefaultEndpoints();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/items", () =>
{
    return Results.Ok(new[]
    {
        new { id = 1, name = "Item One" },
        new { id = 2, name = "Item Two" },
    });
});

app.MapGet("/api/profile", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated != true)
        return Results.Unauthorized();

    var email = user.FindFirstValue(ClaimTypes.Email) ?? user.FindFirstValue("email") ?? "unknown";
    var displayName = user.FindFirstValue(ClaimTypes.Name) ?? email;

    return Results.Ok(new { email, displayName });
}).RequireAuthorization();

app.Run();
