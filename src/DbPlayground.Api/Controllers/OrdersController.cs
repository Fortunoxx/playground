using DbPlayground.Api.Data;
using DbPlayground.Api.Models;
using DbPlayground.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refit;
using System.Text.Json;

namespace DbPlayground.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(CustomerDbContext dbContext, IRulesApi rulesApi) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Order>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await dbContext.Orders.AsNoTracking().OrderBy(order => order.Id).ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Order>> GetById(int id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.AsNoTracking().SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> Create(OrderRequest request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.AsNoTracking().SingleOrDefaultAsync(customer => customer.Id == request.CustomerId, cancellationToken);
        var product = await dbContext.Products.Include(product => product.Rules).SingleOrDefaultAsync(product => product.Id == request.ProductId, cancellationToken);
        if (customer is null || product is null) return NotFound("Customer or product was not found.");

        var evaluatedAtUtc = DateTime.UtcNow;
        KieServerResponse evaluation;
        try
        {
            evaluation = await rulesApi.EvaluateAsync("order-rules", new KieServerCommandRequest
            {
                Lookup = "order-rules-session",
                    Commands =
                    [
                    new KieServerCommand { Insert = new KieInsertCommand { Object = new { birthDate = customer.BirthDate, rules = product.Rules.Where(rule => rule.IsActive).Select(rule => new { rule.MinimumAge, rule.MaximumAge, rule.ValidFromUtc, rule.ValidToUtc }) } } },
                    new KieServerCommand { FireAllRules = new { } }
                    ]
            }, cancellationToken);
        }
        catch (ApiException exception)
        {
            return Problem(detail: exception.Message, title: "The rules service rejected the evaluation request.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var allowed = evaluation.Result.ValueKind == JsonValueKind.Object && evaluation.Result.TryGetProperty("allowed", out var allowedProperty) && allowedProperty.GetBoolean();
        if (!allowed) return UnprocessableEntity(new { message = "Drools did not authorize this order." });

        var order = new Order { CustomerId = customer.Id, ProductId = product.Id, Quantity = request.Quantity, UnitPrice = product.Price, CreatedAtUtc = evaluatedAtUtc };
        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
        if (order is null) return NotFound();
        dbContext.Orders.Remove(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}