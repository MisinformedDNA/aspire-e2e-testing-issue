using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Clerk.Blazor;

public static class ClerkServiceCollectionExtensions
{
    public static IServiceCollection AddClerkAuth(this IServiceCollection services)
    {
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddScoped<AuthenticationStateProvider, ClerkAuthenticationStateProvider>();
        return services;
    }
}
