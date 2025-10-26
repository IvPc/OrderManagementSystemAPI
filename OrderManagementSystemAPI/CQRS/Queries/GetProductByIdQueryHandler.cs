using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Queries;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    private readonly IProductRepository _repository;

    public GetProductByIdQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetProductByIdResult> HandleAsync(GetProductByIdQuery query)
    {
        var product = await _repository.GetByIdAsync(query.Id);
        return new GetProductByIdResult(product);
    }
}