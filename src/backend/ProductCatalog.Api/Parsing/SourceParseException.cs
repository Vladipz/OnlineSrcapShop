namespace ProductCatalog.Api.Parsing;

public sealed class SourceParseException(string message, Exception? innerException = null)
    : Exception(message, innerException);
