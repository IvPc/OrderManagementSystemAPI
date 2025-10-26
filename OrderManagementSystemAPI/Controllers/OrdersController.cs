using Microsoft.AspNetCore.Mvc;
using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Models.DTOs;

namespace OrderManagementSystemAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IQueryDispatcher _queryDispatcher;

    public OrdersController(ICommandDispatcher commandDispatcher, IQueryDispatcher queryDispatcher)
    {
        _commandDispatcher = commandDispatcher;
        _queryDispatcher = queryDispatcher;
    }
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _queryDispatcher.DispatchAsync<GetOrderByIdQuery, GetOrderByIdResult>(query);

        if (result.Order == null)
            return NotFound(new { message = $"Product with ID {id} not found" });

        return Ok(result.Order);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var command = new CreateOrderCommand(dto.Items);
        var result = await _commandDispatcher.DispatchAsync<CreateOrderCommand, CreateOrderResult>(command);

        return CreatedAtAction(nameof(Create), new { id = result.OrderSummary.OrderId }, result.OrderSummary);
    }
}
