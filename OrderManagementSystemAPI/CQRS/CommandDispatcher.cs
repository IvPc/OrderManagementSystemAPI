using OrderManagementSystemAPI.CQRS.Interfaces;

namespace OrderManagementSystemAPI.CQRS;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> DispatchAsync<TCommand, TResult>(TCommand command)
        where TCommand : class
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(typeof(TCommand), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
            throw new InvalidOperationException($"No handler registered for command {typeof(TCommand).Name}");

        var method = handlerType.GetMethod("HandleAsync");
        if (method == null)
            throw new InvalidOperationException($"HandleAsync method not found on handler for {typeof(TCommand).Name}");

        var result = method.Invoke(handler, new object[] { command });
        if (result is Task<TResult> task)
            return await task;

        throw new InvalidOperationException($"Handler for {typeof(TCommand).Name} did not return expected type");
    }
}