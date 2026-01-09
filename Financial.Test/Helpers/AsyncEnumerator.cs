namespace Financial.Test.Helpers
{
    /// <summary>
    /// Async enumerator for mocking async queries in EF Core.
    /// </summary>
    internal class AsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public AsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public async ValueTask<bool> MoveNextAsync()
        {
            return await Task.FromResult(_inner.MoveNext());
        }

        public async ValueTask DisposeAsync()
        {
            await Task.FromResult(0);
            _inner.Dispose();
        }
    }
}
