using Microsoft.EntityFrameworkCore.Query;

namespace Financial.Test.Helpers
{
    /// <summary>
    /// Async query provider for mocking async queries in EF Core.
    /// </summary>
    internal class AsyncQueryProvider<T> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public AsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            var innerQuery = _inner.CreateQuery(expression);
            var queryableType = typeof(AsyncQueryable<>).MakeGenericType(innerQuery.ElementType);
            return (IQueryable)Activator.CreateInstance(queryableType, innerQuery)!;
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new AsyncQueryable<TElement>(_inner.CreateQuery<TElement>(expression));
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression)
        {
            return new AsyncEnumerable<TResult>(_inner.CreateQuery<TResult>(expression));
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken)
        {
            var enumerable = ExecuteAsync<object>(expression);
            var task = Task.FromResult((TResult)(object)enumerable);
            return task.Result;
        }
    }
}
