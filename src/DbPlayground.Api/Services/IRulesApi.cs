using Refit;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DbPlayground.Api.Services;

public interface IRulesApi
{
    [Post("/containers/instances/{containerId}")]
    Task<KieServerResponse> EvaluateAsync(string containerId, [Body] KieServerCommandRequest request, CancellationToken cancellationToken = default);
}

public sealed class KieServerCommandRequest
{
    public required string Lookup { get; set; }
    public required KieServerCommand[] Commands { get; set; }
}

public sealed class KieServerCommand
{
    [JsonPropertyName("insert")]
    public KieInsertCommand? Insert { get; set; }

    [JsonPropertyName("fire-all-rules")]
    public object? FireAllRules { get; set; }
}

public sealed class KieInsertCommand
{
    [JsonPropertyName("object")]
    public required object Object { get; set; }

    [JsonPropertyName("out-identifier")]
    public string? OutIdentifier { get; set; }
}

public sealed class KieServerResponse
{
    [JsonPropertyName("result")]
    public JsonElement Result { get; set; }
}