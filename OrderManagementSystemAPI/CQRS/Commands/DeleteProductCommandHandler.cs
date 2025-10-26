using OrderManagementSystemAPI.CQRS.Interfaces;
using OrderManagementSystemAPI.Models.CQRS.Commands;
using OrderManagementSystemAPI.Repositories.Interfaces;

namespace OrderManagementSystemAPI.CQRS.Commands;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    private readonly IProductRepository _repository;

    public DeleteProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteProductResult> HandleAsync(DeleteProductCommand command)
    {
        var product = await _repository.GetByIdAsync(command.Id);
        if (product == null) return new DeleteProductResult(false);

        await _repository.DeleteAsync(command.Id);

        return new DeleteProductResult(true);
    }
}