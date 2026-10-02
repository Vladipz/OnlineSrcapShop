using ProductCatalog.Api.Auth;
using ProductCatalog.Api.Common;
using ProductCatalog.Api.Common.ErrorHandling;
using ProductCatalog.Api.Data;
using ProductCatalog.Api.Imports;
using ProductCatalog.Api.Products;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddDataProtectionConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddApplicationIdentity(builder.Configuration);
builder.Services.AddCatalogFeatures(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages(context => Results.Problem(statusCode: context.HttpContext.Response.StatusCode)
    .ExecuteAsync(context.HttpContext));
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapProductEndpoints();
app.MapImportEndpoints();
app.MapGet("/health", () => Results.NoContent()).AllowAnonymous().ExcludeFromDescription();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.InitializeDatabaseAsync(app.Lifetime.ApplicationStopping);

app.Run();
