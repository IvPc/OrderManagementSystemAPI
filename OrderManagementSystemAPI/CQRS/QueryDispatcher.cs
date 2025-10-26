using OrderManagementSystemAPI.CQRS.Interfaces;

namespace OrderManagementSystemAPI.CQRS;

public class QueryDispatcher : IQueryDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public QueryDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query)
        where TQuery : class
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(typeof(TQuery), typeof(TResult));
        var handler = _serviceProvider.GetService(handlerType);

        if (handler == null)
            throw new InvalidOperationException($"No handler registered for query {typeof(TQuery).Name}");

        var method = handlerType.GetMethod("HandleAsync");
        if (method == null)
            throw new InvalidOperationException($"HandleAsync method not found on handler for {typeof(TQuery).Name}");

        var result = method.Invoke(handler, new object[] { query });
        if (result is Task<TResult> task)
            return await task;

        throw new InvalidOperationException($"Handler for {typeof(TQuery).Name} did not return expected type");
    }
}