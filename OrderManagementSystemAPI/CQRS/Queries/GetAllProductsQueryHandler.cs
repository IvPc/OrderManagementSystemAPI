using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Queries;

public class GetAllProductsQueryHandler : IQueryHandler<GetAllProductsQuery, GetAllProductsResult>
{
    private readonly IProductRepository _repository;

    public GetAllProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetAllProductsResult> HandleAsync(GetAllProductsQuery query)
    {
        var products = await _repository.GetAllAsync();

        return new GetAllProductsResult(products);
    }
}