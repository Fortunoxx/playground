using RulesEngine.Models;
using System.Text.Json;

namespace DbPlayground.Api.Services;

public interface IRulesEngineEvaluator
{
    Task<OrderRuleDecision> EvaluateAsync(OrderRuleInput input, CancellationToken cancellationToken = default);
}

public sealed class OrderRuleInput
{
    public DateTime? BirthDate { get; init; }
    public DateTime? EvaluationAtUtc { get; init; }
}

public sealed record OrderRuleDecision(bool Allowed, string Reason);

public sealed class RulesEngineEvaluator(IConfiguration configuration, IWebHostEnvironment environment) : IRulesEngineEvaluator
{
    private readonly Lazy<RulesEngine.RulesEngine> rulesEngine = new(() => CreateRulesEngine(configuration, environment));

    public async Task<OrderRuleDecision> EvaluateAsync(OrderRuleInput input, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflowName = configuration["RuleEngine:WorkflowName"] ?? "OrderWorkflow";
        var results = await rulesEngine.Value.ExecuteAllRulesAsync(workflowName, new RuleParameter("order", input));
        var result = results.FirstOrDefault(rule => rule.IsSuccess);

        return result?.Rule.RuleName switch
        {
            "Allow adult customer" => new OrderRuleDecision(true, "Customer is at least 18 years old."),
            "Deny non-adult customer" => new OrderRuleDecision(false, "Customer must be at least 18 years old."),
            "Deny invalid order fact" => new OrderRuleDecision(false, "The order fact is missing birthDate or evaluationAtUtc."),
            _ => new OrderRuleDecision(false, "No order rule authorized this order.")
        };
    }

    private static RulesEngine.RulesEngine CreateRulesEngine(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var rulesFile = configuration["RuleEngine:RulesFile"] ?? "rules.json";
        var rulesPath = Path.Combine(environment.ContentRootPath, rulesFile);
        var workflows = JsonSerializer.Deserialize<Workflow[]>(File.ReadAllText(rulesPath))
            ?? throw new InvalidOperationException($"The rules file '{rulesPath}' does not contain any workflows.");
        return new RulesEngine.RulesEngine(workflows);
    }
}