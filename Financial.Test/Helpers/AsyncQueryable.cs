namespace Financial.Test.Helpers
{
    /// <summary>
    /// Async queryable wrapper for mocking async queries in EF Core.
    /// </summary>
    internal class AsyncQueryable<T> : IAsyncEnumerable<T>, IQueryable<T>
    {
        private readonly IQueryable<T> _inner;

        public AsyncQueryable(IQueryable<T> inner)
        {
            _inner = inner;
        }

        public System.Linq.Expressions.Expression Expression => _inner.Expression;
        public Type ElementType => _inner.ElementType;
        public IQueryProvider Provider
        {
            get
            {
                // If the inner provider is already AsyncQueryProvider, return it as-is
                if (_inner.Provider is AsyncQueryProvider<T>)
                    return _inner.Provider;
                // Otherwise wrap it
                return new AsyncQueryProvider<T>(_inner.Provider);
            }
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerator<T>(_inner.GetEnumerator());
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }
    }
}
