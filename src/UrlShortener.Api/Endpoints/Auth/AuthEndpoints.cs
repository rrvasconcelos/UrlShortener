using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using UrlShortener.Infrastructure.Identity;
using UrlShortener.Infrastructure.Services.Auth;

namespace UrlShortener.Api.Endpoints.Auth;

public class AuthEndpoints : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/register", RegisterAsync).AllowAnonymous();
        app.MapPost("/login", LoginAsync).AllowAnonymous();
        app.MapPost("/refresh", RefreshAsync).AllowAnonymous();
        app.MapPost("/logout", LogoutAsync).RequireAuthorization();
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        UserManager<ApplicationUser> userManager)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return Results.BadRequest(new { Errors = errors });
        }

        return Results.Ok(new { Message = "User registered successfully" });
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Results.Unauthorized();

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
            return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays);
        await userManager.UpdateAsync(user);

        return Results.Ok(new AuthResponse(accessToken, refreshToken));
    }

    private static async Task<IResult> RefreshAsync(
        RefreshTokenRequest request,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Results.Unauthorized();

        var roles = await userManager.GetRolesAsync(user);
        var newAccessToken = jwtTokenService.GenerateAccessToken(user, roles);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(jwtOptions.Value.RefreshTokenExpirationDays);
        await userManager.UpdateAsync(user);

        return Results.Ok(new AuthResponse(newAccessToken, newRefreshToken));
    }

    private static async Task<IResult> LogoutAsync(
        UserManager<ApplicationUser> userManager,
        HttpContext httpContext)
    {
        var userId = userManager.GetUserId(httpContext.User);
        if (userId is null)
            return Results.Unauthorized();

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
            return Results.Unauthorized();

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userManager.UpdateAsync(user);

        return Results.Ok(new { Message = "Logged out successfully" });
    }
}

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string UserId, string RefreshToken);
public record AuthResponse(string AccessToken, string RefreshToken);
