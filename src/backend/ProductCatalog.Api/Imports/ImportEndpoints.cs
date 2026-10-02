using ProductCatalog.Api.Auth;
using ProductCatalog.Api.Parsing;
using ProductCatalog.Api.Products;

namespace ProductCatalog.Api.Imports;

public static class ImportEndpoints
{
    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/import").WithTags("Import")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapPost("/", ImportAsync)
            .WithName("ImportProducts").Produces<ImportResponse>().ProducesProblem(StatusCodes.Status409Conflict);

        group.ProducesValidationProblem().ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden).ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status502BadGateway);
        return endpoints;
    }

    private static async Task<IResult> ImportAsync(
        ImportRequest request, ProductImportService service, CancellationToken ct)
    {
        if (!ProductValidation.IsHttpUrl(request.SourceUrl))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["sourceUrl"] = ["An absolute HTTP/HTTPS source URL of at most 2048 characters is required."]
            });
        }

        var uri = SourceUrlPolicy.Normalize(new Uri(request.SourceUrl!, UriKind.Absolute));
        if (!SourceUrlPolicy.IsSupported(uri))
        {
            return Results.Problem(statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Unsupported source", detail: "The URL must belong to a supported store and use its standard HTTP/HTTPS port.");
        }

        return Results.Ok(await service.ImportAsync(uri, ct));
    }
}
