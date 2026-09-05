using System.ComponentModel.DataAnnotations;

namespace DbPlayground.Api.Models;

public sealed class CustomerRequest
{
    [Required]
    [StringLength(120)]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(320)]
    public required string Email { get; set; }

    [Phone]
    [StringLength(40)]
    public string? Phone { get; set; }
}