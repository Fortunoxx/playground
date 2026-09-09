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
    [JsonPropertyName("lookup")]
    public required string Lookup { get; set; }

    [JsonPropertyName("commands")]
    public required object[] Commands { get; set; }
}

public sealed class KieServerCommand
{
    [JsonPropertyName("insert")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public KieInsertCommand? Insert { get; set; }

    [JsonPropertyName("set-global")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public KieSetGlobalCommand? SetGlobal { get; set; }

    [JsonPropertyName("fire-all-rules")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? FireAllRules { get; set; }

    [JsonPropertyName("get-global")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public KieGetGlobalCommand? GetGlobal { get; set; }
}

public sealed class KieInsertCommand
{
    [JsonPropertyName("object")]
    public required object Object { get; set; }

    [JsonPropertyName("out-identifier")]
    public string? OutIdentifier { get; set; }
}

public sealed class KieSetGlobalCommand
{
    [JsonPropertyName("identifier")]
    public required string Identifier { get; set; }

    [JsonPropertyName("object")]
    public required object Object { get; set; }
}

public sealed class KieGetGlobalCommand
{
    [JsonPropertyName("identifier")]
    public required string Identifier { get; set; }

    [JsonPropertyName("out-identifier")]
    public required string OutIdentifier { get; set; }
}

public sealed class KieServerResponse
{
    [JsonPropertyName("result")]
    public JsonElement Result { get; set; }
}