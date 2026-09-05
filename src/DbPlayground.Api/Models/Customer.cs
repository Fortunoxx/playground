namespace DbPlayground.Api.Models;

public sealed class Customer
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string Email { get; set; }

    public string? Phone { get; set; }

    public DateOnly BirthDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}