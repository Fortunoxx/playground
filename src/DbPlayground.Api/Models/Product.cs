namespace DbPlayground.Api.Models;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public List<ProductRule> Rules { get; set; } = [];
}