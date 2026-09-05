using System.ComponentModel.DataAnnotations;

namespace DbPlayground.Api.Models;

public sealed class ProductRuleRequest
{
    [Range(0, 150)]
    public int? MinimumAge { get; set; }
    [Range(0, 150)]
    public int? MaximumAge { get; set; }
    public DateTime ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public bool IsActive { get; set; } = true;
}