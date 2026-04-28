using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.Security.Claims;

namespace Clerk.Blazor;

public class ClerkAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private static readonly AuthenticationState Unauthenticated = new(new ClaimsPrincipal(new ClaimsIdentity()));
    private DotNetObjectReference<ClerkAuthenticationStateProvider>? _dotNetRef;

    public ClerkAuthenticationStateProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            var userInfo = await _jsRuntime.InvokeAsync<ClerkUserInfo?>("clerkInterop.getUserInfo");
            if (userInfo is null || string.IsNullOrEmpty(userInfo.Email))
                return Unauthenticated;

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, userInfo.Email),
                new(ClaimTypes.Name, userInfo.DisplayName ?? userInfo.Email),
                new("sub", userInfo.UserId ?? string.Empty),
            };

            var identity = new ClaimsIdentity(claims, "Clerk");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return Unauthenticated;
        }
    }

    [JSInvokable]
    public void OnAuthStateChanged(ClerkUserInfo? userInfo)
    {
        NotifyAuthenticationStateChanged(Task.FromResult(
            userInfo is not null && !string.IsNullOrEmpty(userInfo.Email)
                ? new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.Email, userInfo.Email),
                        new Claim(ClaimTypes.Name, userInfo.DisplayName ?? userInfo.Email),
                        new Claim("sub", userInfo.UserId ?? string.Empty),
                    ], "Clerk")))
                : Unauthenticated));
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}

public record ClerkUserInfo(string? UserId, string? Email, string? DisplayName);
