using ProductCatalog.Api.Auth;

namespace ProductCatalog.Api.Products;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products").WithTags("Products")
            .RequireAuthorization(AuthorizationPolicies.CatalogRead);

        group.MapGet("/", async (ProductService service, CancellationToken ct) =>
            Results.Ok(await service.GetAllAsync(ct)))
            .WithName("GetProducts").Produces<ProductResponse[]>();

        group.MapGet("/{id:int}", async (int id, ProductService service, CancellationToken ct) =>
            await service.GetAsync(id, ct) is { } product
                ? Results.Ok(product) : Results.Problem(statusCode: StatusCodes.Status404NotFound))
            .WithName("GetProduct").Produces<ProductResponse>().ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:int}", UpdateAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("UpdateProduct").Produces<ProductResponse>().ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", async (int id, ProductService service, CancellationToken ct) =>
            await service.DeleteAsync(id, ct)
                ? Results.NoContent() : Results.Problem(statusCode: StatusCodes.Status404NotFound))
            .RequireAuthorization(AuthorizationPolicies.AdminOnly)
            .WithName("DeleteProduct").Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status403Forbidden);
        return endpoints;
    }

    private static async Task<IResult> UpdateAsync(
        int id, UpdateProductRequest request, ProductService service, CancellationToken ct)
    {
        var errors = ProductValidation.Validate(request);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        return await service.UpdateAsync(id, request, ct) is { } product
            ? Results.Ok(product) : Results.Problem(statusCode: StatusCodes.Status404NotFound);
    }
}
