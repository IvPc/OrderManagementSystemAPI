using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Commands;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateProductResult> HandleAsync(CreateProductCommand command)
    {
        var product = new Product
        {
            Name = command.Name,
            Price = command.Price,
            StockQuantity = command.StockQuantity
        };

        var createdProduct = await _repository.AddAsync(product);

        return new CreateProductResult(createdProduct);
    }
}