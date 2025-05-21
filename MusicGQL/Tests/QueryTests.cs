using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using MusicGQL.Db.Postgres;
using MusicGQL.Db.Postgres.Models;
using MusicGQL.Types;
using Xunit;

namespace MusicGQL.Tests
{
    public class QueryTests
    {
        private Mock<EventDbContext> _mockDbContext;
        private Mock<DbSet<UserProjection>> _mockUserProjectionsDbSet;

        private void SetupMocks(List<UserProjection> users)
        {
            var queryableUsers = users.AsQueryable();

            _mockUserProjectionsDbSet = new Mock<DbSet<UserProjection>>();
            _mockUserProjectionsDbSet.As<IAsyncEnumerable<UserProjection>>()
                .Setup(m => m.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<UserProjection>(queryableUsers.GetEnumerator()));
            _mockUserProjectionsDbSet.As<IQueryable<UserProjection>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<UserProjection>(queryableUsers.Provider));
            _mockUserProjectionsDbSet.As<IQueryable<UserProjection>>().Setup(m => m.Expression).Returns(queryableUsers.Expression);
            _mockUserProjectionsDbSet.As<IQueryable<UserProjection>>().Setup(m => m.ElementType).Returns(queryableUsers.ElementType);
            _mockUserProjectionsDbSet.As<IQueryable<UserProjection>>().Setup(m => m.GetEnumerator()).Returns(() => queryableUsers.GetEnumerator());

            _mockDbContext = new Mock<EventDbContext>();
            _mockDbContext.Setup(db => db.UserProjections).Returns(_mockUserProjectionsDbSet.Object);
        }

        [Fact]
        public async Task GetAreThereAnyUsers_ReturnsFalse_WhenNoUsersExist()
        {
            // Arrange
            var users = new List<UserProjection>();
            SetupMocks(users);
            var queryResolver = new Query();

            // Act
            var result = await queryResolver.GetAreThereAnyUsers(_mockDbContext.Object);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAreThereAnyUsers_ReturnsTrue_WhenUsersExist()
        {
            // Arrange
            var users = new List<UserProjection> { new UserProjection { UserId = System.Guid.NewGuid(), Username = "testuser" } };
            SetupMocks(users);
            var queryResolver = new Query();

            // Act
            var result = await queryResolver.GetAreThereAnyUsers(_mockDbContext.Object);

            // Assert
            Assert.True(result);
        }
    }

    // Helper classes for mocking IAsyncEnumerable and IQueryable for EF Core
    // These are standard helpers used when mocking EF Core operations.
    // Source: https://docs.microsoft.com/en-us/ef/core/testing/mocking#async-operations
    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public T Current => _inner.Current;
    }
}
