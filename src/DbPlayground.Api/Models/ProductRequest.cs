using System.ComponentModel.DataAnnotations;

namespace DbPlayground.Api.Models;

public sealed class ProductRequest
{
    [Required, StringLength(160)]
    public required string Name { get; set; }
    [StringLength(2000)]
    public string? Description { get; set; }
    [Range(typeof(decimal), "0.01", "1000000000")]
    public decimal Price { get; set; }
}