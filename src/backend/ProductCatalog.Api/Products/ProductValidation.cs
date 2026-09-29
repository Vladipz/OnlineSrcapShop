namespace ProductCatalog.Api.Products;

public static class ProductValidation
{
    public static Dictionary<string, string[]> Validate(UpdateProductRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length > 300)
        {
            errors["name"] = ["Name is required and must not exceed 300 characters."];
        }

        if (request.Description?.Length > 5000)
        {
            errors["description"] = ["Description must not exceed 5000 characters."];
        }

        if (!IsHttpUrl(request.ImageUrl))
        {
            errors["imageUrl"] = ["An absolute HTTP/HTTPS image URL of at most 2048 characters is required."];
        }

        if (request.Price < 0)
        {
            errors["price"] = ["Price must be nonnegative."];
        }

        if (request.CurrencyCode?.Length > 3)
        {
            errors["currencyCode"] = ["Currency code must not exceed 3 characters."];
        }

        return errors;
    }

    public static bool IsHttpUrl(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Length <= 2048
        && Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https"
        && !string.IsNullOrEmpty(uri.Host)
        && string.IsNullOrEmpty(uri.UserInfo);
}
