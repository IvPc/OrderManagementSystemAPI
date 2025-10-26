using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Commands;

public class SoftDeleteProductCommandHandler : ICommandHandler<SoftDeleteProductCommand, DeleteProductResult>
{
    private readonly IProductRepository _repository;

    public SoftDeleteProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteProductResult> HandleAsync(SoftDeleteProductCommand command)
    {
        var product = await _repository.GetByIdAsync(command.Id);
        
        if (product == null) return new DeleteProductResult(false);

        await _repository.SoftDeleteAsync(command.Id);

        return new DeleteProductResult(true);
    }
}