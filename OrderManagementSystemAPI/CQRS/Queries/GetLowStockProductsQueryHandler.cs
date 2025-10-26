using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Queries;
using OrderManagementSystemAPI.Models.DTOs;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Queries;

public class GetLowStockProductsQueryHandler : IQueryHandler<GetLowStockProductsQuery, GetLowStockProductsResult>
{
    private readonly IProductRepository _repository;

    public GetLowStockProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetLowStockProductsResult> HandleAsync(GetLowStockProductsQuery query)
    {
        var products = await _repository.GetLowStockProductsAsync(query.Threshold);

        var dtos = products.Select(p => new LowStockProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            StockQuantity = p.StockQuantity
        });

        return new GetLowStockProductsResult(dtos);
    }
}