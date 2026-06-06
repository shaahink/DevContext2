namespace DevContext.Core.Constants;

public static class HttpConstants
{
    public static readonly ImmutableArray<string> MapMethods =
        ["MapGet", "MapPost", "MapPut", "MapDelete", "MapPatch"];

    public static readonly ImmutableArray<string> HttpVerbAttributes =
        ["HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch"];

    public static readonly ImmutableDictionary<string, string> MapMethodToVerb =
        ImmutableDictionary.CreateRange(StringComparer.OrdinalIgnoreCase, new[]
        {
            KeyValuePair.Create("MapGet", "GET"),
            KeyValuePair.Create("MapPost", "POST"),
            KeyValuePair.Create("MapPut", "PUT"),
            KeyValuePair.Create("MapDelete", "DELETE"),
            KeyValuePair.Create("MapPatch", "PATCH"),
        });
}
