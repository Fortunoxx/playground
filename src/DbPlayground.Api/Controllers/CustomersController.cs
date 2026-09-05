using DbPlayground.Api.Data;
using DbPlayground.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbPlayground.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController(CustomerDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Customer>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await dbContext.Customers.AsNoTracking().OrderBy(customer => customer.Id).ToListAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Customer>> GetById(int id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<Customer>> Create(CustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = new Customer
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            BirthDate = request.BirthDate,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Customers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CustomerRequest request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        customer.Name = request.Name;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.BirthDate = request.BirthDate;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);
        if (customer is null)
        {
            return NotFound();
        }

        dbContext.Customers.Remove(customer);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}