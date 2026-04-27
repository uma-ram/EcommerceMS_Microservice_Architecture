using Microsoft.AspNetCore.Mvc;
using ProductService.DTOs;
using ProductService.Services;

namespace ProductService.Controllers;

[ApiController]
[Route("products")]
public class ProductsController:ControllerBase
{
    private readonly ProductCatalogService _service;

    public ProductsController(ProductCatalogService service)
    {
        _service = service;
    }

    // GET /products
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAllAsync();
        return Ok(products);
    }

    //GET /products/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var product= await _service.GetByIdAsync(id);
        return product is null ? NotFound() : Ok(product);
    }

    // POST /products
    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var product = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    // PUT /products/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var product = await _service.UpdateAsync(id, request);
        return product is null ? NotFound() : Ok(product);
    }

    // DELETE /products/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result ? NoContent() : NotFound();
    }

    // PUT /products/5/reduce-stock  (called by Order Service later)
    [HttpPut("{id}/reduce-stock")]
    public async Task<IActionResult> ReduceStock(int id, [FromBody] int quantity)
    {
        var result = await _service.ReduceStockAsync(id, quantity);
        return result ? Ok() : BadRequest("Insufficient stock or product not found");
    }
}
