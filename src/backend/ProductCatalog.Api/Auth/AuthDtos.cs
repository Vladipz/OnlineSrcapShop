namespace ProductCatalog.Api.Auth;

public sealed record LoginRequest(string? Email, string? Password);

public sealed record CurrentUserResponse(string Id, string? Email, IReadOnlyList<string> Roles);
