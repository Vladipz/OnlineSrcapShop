using ProductCatalog.Api.Common;
using ProductCatalog.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase(builder.Configuration, builder.Environment);
builder.Services.AddApplicationIdentity(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.InitializeDatabaseAsync(app.Lifetime.ApplicationStopping);

// Authentication policies and /api endpoints will be added in subsequent iterations.
app.Run();
