using System.ComponentModel.DataAnnotations;

namespace DbPlayground.Api.Models;

public sealed class OrderRequest
{
    [Range(1, int.MaxValue)]
    public int CustomerId { get; set; }
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }
    [Range(1, 100000)]
    public int Quantity { get; set; }
}