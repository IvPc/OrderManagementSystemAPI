namespace OrderManagementSystemAPI.CQRS.Interfaces;

public interface IQueryDispatcher
{
    Task<TResult> DispatchAsync<TQuery, TResult>(TQuery query)
        where TQuery : class;
}