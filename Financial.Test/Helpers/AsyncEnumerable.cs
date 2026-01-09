namespace Financial.Test.Helpers
{
    /// <summary>
    /// Async enumerable wrapper for mocking async queries in EF Core.
    /// </summary>
    internal class AsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _inner;

        public AsyncEnumerable(IEnumerable<T> inner)
        {
            _inner = inner;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new AsyncEnumerator<T>(_inner.GetEnumerator());
        }
    }
}
