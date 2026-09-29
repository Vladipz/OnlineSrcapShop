namespace ProductCatalog.Api.Auth;

public sealed class SeedUsersOptions
{
    public const string SectionName = "SeedUsers";

    public bool Enabled { get; init; }

    public SeedAccountOptions Admin { get; init; } = new();

    public SeedAccountOptions User { get; init; } = new();
}

public sealed class SeedAccountOptions
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
