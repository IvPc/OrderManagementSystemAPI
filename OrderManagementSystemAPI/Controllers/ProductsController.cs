using Microsoft.AspNetCore.Mvc;
using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Models.DTOs;

namespace OrderManagementSystemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public ProductsController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var query = new GetAllProductsQuery();
        var result = await _queryDispatcher.DispatchAsync<GetAllProductsQuery, GetAllProductsResult>(query);
        return Ok(result.Products);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _queryDispatcher.DispatchAsync<GetProductByIdQuery, GetProductByIdResult>(query);

        if (result.Product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(result.Product);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var command = new CreateProductCommand(dto.Name, dto.Price, dto.StockQuantity);
        var result = await _commandDispatcher.DispatchAsync<CreateProductCommand, CreateProductResult>(command);

        return CreatedAtAction(nameof(GetById), new { id = result.Product.Id }, result.Product);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var command = new UpdateProductCommand(id, dto.Name, dto.Price, dto.StockQuantity);
        var result = await _commandDispatcher.DispatchAsync<UpdateProductCommand, UpdateProductResult>(command);

        if (result.Product == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(result.Product);
    }

    [HttpDelete("{id}/soft")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SoftDelete(int id)
    {
        var command = new SoftDeleteProductCommand(id);
        var result = await _commandDispatcher.DispatchAsync<SoftDeleteProductCommand, DeleteProductResult>(command);

        if (!result.Success)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var command = new DeleteProductCommand(id);
        var result = await _commandDispatcher.DispatchAsync<DeleteProductCommand, DeleteProductResult>(command);

        if (!result.Success)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return NoContent();
    }
}
