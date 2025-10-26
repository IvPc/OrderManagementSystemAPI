using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Commands;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IProductRepository _repository;

    public UpdateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<UpdateProductResult> HandleAsync(UpdateProductCommand command)
    {
        var product = await _repository.GetByIdAsync(command.Id);
        if (product == null) return new UpdateProductResult(null);

        product.Name = command.Name;
        product.Price = command.Price;
        product.StockQuantity = command.StockQuantity;

        await _repository.UpdateAsync(product);

        return new UpdateProductResult(product);
    }
}