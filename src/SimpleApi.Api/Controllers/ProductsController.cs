using Microsoft.AspNetCore.Mvc;
using SimpleApi.Application.DTOs;
using SimpleApi.Application.Services;
using SimpleApi.Domain.Entities;

namespace SimpleApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _productService.GetAll();
        return Ok(products.Select(p => MapToDto(p)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _productService.GetById(id);
        return Ok(MapToDto(product));
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto createDto)
    {
        var product = new Product { Name = createDto.Name, Price = createDto.Price };
        var createdProduct = await _productService.Create(product);
        return CreatedAtAction(nameof(GetById), new { id = createdProduct.Id }, MapToDto(createdProduct));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> Update(int id, [FromBody] UpdateProductDto updateDto)
    {
        var product = new Product { Name = updateDto.Name, Price = updateDto.Price };
        var updatedProduct = await _productService.Update(id, product);
        return Ok(MapToDto(updatedProduct));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.Delete(id);
        return NoContent();
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        };
    }
}
