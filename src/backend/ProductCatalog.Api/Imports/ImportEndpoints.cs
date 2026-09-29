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

        group.MapPost("/preview", (ImportRequest request, ProductParserResolver resolver,
            ProductImportService service, CancellationToken ct) => HandleAsync(request, resolver, service, false, ct))
            .WithName("PreviewImport").Produces<ImportPreviewResponse>();

        group.MapPost("/confirm", (ImportRequest request, ProductParserResolver resolver,
            ProductImportService service, CancellationToken ct) => HandleAsync(request, resolver, service, true, ct))
            .WithName("ConfirmImport").Produces<ImportConfirmResponse>().ProducesProblem(StatusCodes.Status409Conflict);

        group.ProducesValidationProblem().ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden).ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status502BadGateway);
        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        ImportRequest request, ProductParserResolver resolver, ProductImportService service, bool confirm, CancellationToken ct)
    {
        if (!ProductValidation.IsHttpUrl(request.SourceUrl))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["sourceUrl"] = ["An absolute HTTP/HTTPS source URL of at most 2048 characters is required."]
            });
        }

        var uri = SourceUrlPolicy.Normalize(new Uri(request.SourceUrl!, UriKind.Absolute));
        var parser = resolver.Resolve(uri);
        if (parser is null)
        {
            return Results.Problem(statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Unsupported source", detail: "Only books.toscrape.com on its standard HTTP/HTTPS ports is supported.");
        }

        return confirm
            ? Results.Ok(await service.ConfirmAsync(uri, parser, ct))
            : Results.Ok(await service.PreviewAsync(uri, parser, ct));
    }
}
