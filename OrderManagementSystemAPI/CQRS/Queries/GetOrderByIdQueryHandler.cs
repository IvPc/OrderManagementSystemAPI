using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Queries;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, GetOrderByIdResult>
{
    private readonly IOrderRepository _repository;

    public GetOrderByIdQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetOrderByIdResult> HandleAsync(GetOrderByIdQuery query)
    {
        var order = await _repository.GetByIdAsync(query.Id);
        return new GetOrderByIdResult(order);
    }
}