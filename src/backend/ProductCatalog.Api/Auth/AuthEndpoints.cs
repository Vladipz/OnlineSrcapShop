using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Identity;

namespace ProductCatalog.Api.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .AllowAnonymous()
            .WithName("Login")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetCurrentUserAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .Produces<CurrentUserResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .RequireAuthorization()
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        var errors = new Dictionary<string, string[]>();
        var email = request.Email?.Trim();

        if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            errors["password"] = ["A password is required."];
        }

        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var user = await userManager.FindByEmailAsync(email!);

        if (user is not null)
        {
            var result = await signInManager.PasswordSignInAsync(
                user, request.Password!, isPersistent: false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return Results.NoContent();
            }
        }

        return Results.Problem(
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Authentication failed",
            detail: "The email or password is invalid, or the account is unavailable.");
    }

    private static async Task<IResult> GetCurrentUserAsync(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        context.Response.Headers.CacheControl = "no-store";
        var user = await userManager.GetUserAsync(context.User);

        if (user is null)
        {
            await signInManager.SignOutAsync();
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var roles = await userManager.GetRolesAsync(user);
        return Results.Ok(new CurrentUserResponse(user.Id, user.Email, roles.Order().ToArray()));
    }

    private static async Task<IResult> LogoutAsync(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.NoContent();
    }
}
