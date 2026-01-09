using Microsoft.EntityFrameworkCore;
using Moq;

namespace Financial.Test.Helpers
{
    /// <summary>
    /// Extension method to convert IQueryable to a Mock DbSet for testing.
    /// This helper is needed because EF Core operations on mock DbSets require special handling.
    /// </summary>
    internal static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> BuildMockDbSet<T>(this IQueryable<T> queryable) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            var asyncProvider = new AsyncQueryProvider<T>(queryable.Provider);
            
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(() => new AsyncEnumerator<T>(queryable.GetEnumerator()));
            
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(asyncProvider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            
            return mockSet;
        }
    }
}
