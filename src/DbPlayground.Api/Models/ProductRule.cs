namespace DbPlayground.Api.Models;

public sealed class ProductRule
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }
    public DateTime ValidFromUtc { get; set; }
    public DateTime? ValidToUtc { get; set; }
    public bool IsActive { get; set; }
}