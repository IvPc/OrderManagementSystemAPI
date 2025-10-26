namespace OrderManagementSystemAPI.CQRS.Interfaces;

public interface ICommandHandler<in TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}