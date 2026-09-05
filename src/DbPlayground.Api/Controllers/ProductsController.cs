using DbPlayground.Api.Data;
using DbPlayground.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DbPlayground.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(CustomerDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await dbContext.Products.AsNoTracking().Include(product => product.Rules).OrderBy(product => product.Id).ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.AsNoTracking().Include(product => product.Rules).SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> Create(ProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product { Name = request.Name, Description = request.Description, Price = request.Price, CreatedAtUtc = DateTime.UtcNow };
        dbContext.Products.Add(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (product is null) return NotFound();
        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);
        if (product is null) return NotFound();
        dbContext.Products.Remove(product);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/rules")]
    public async Task<ActionResult<ProductRule>> AddRule(int id, ProductRuleRequest request, CancellationToken cancellationToken)
    {
        if (!await dbContext.Products.AnyAsync(product => product.Id == id, cancellationToken)) return NotFound();
        var rule = new ProductRule { ProductId = id, MinimumAge = request.MinimumAge, MaximumAge = request.MaximumAge, ValidFromUtc = request.ValidFromUtc, ValidToUtc = request.ValidToUtc, IsActive = request.IsActive };
        dbContext.ProductRules.Add(rule);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(rule);
    }
}