namespace OrderManagementSystemAPI.CQRS.Interfaces;

public interface ICommandDispatcher
{
    Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command)
        where TCommand : class;
}
