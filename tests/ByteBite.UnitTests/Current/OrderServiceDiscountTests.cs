using System.Collections;
using System.Linq.Expressions;
using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace ByteBite.UnitTests.Current;

public class OrderServiceDiscountTests
{
    [Fact]
    public async Task CreateOrderAsync_FullReductionAppliesOnlyOnceForLargeSubtotal()
    {
        var now = DateTime.UtcNow;
        var storeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var fullReductionRuleId = Guid.NewGuid();

        var store = new Store
        {
            Id = storeId,
            MerchantId = Guid.NewGuid(),
            Name = "测试门店",
            StoreCode = "000001",
            BusinessStatus = "open",
            DiningMode = "dine_in,takeaway,delivery",
            PackingFee = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        var product = new Product
        {
            Id = productId,
            StoreId = storeId,
            CategoryId = categoryId,
            Name = "羊肉串",
            BasePrice = 20,
            Status = "on",
            MinOrderQty = 1,
            CreatedAt = now,
            UpdatedAt = now
        };
        var discountRules = new List<DiscountRule>
        {
            new()
            {
                Id = fullReductionRuleId,
                StoreId = storeId,
                Name = "满100减10",
                DiscountType = "full_reduction",
                ThresholdAmount = 100,
                DiscountAmount = 10,
                ApplyScope = "all",
                StartTime = now.AddDays(-1),
                EndTime = now.AddDays(1),
                Status = "active",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                Name = "满200减30",
                DiscountType = "full_reduction",
                ThresholdAmount = 200,
                DiscountAmount = 30,
                ApplyScope = "all",
                StartTime = now.AddDays(-1),
                EndTime = now.AddDays(1),
                Status = "inactive",
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                Name = "烧烤类8折",
                DiscountType = "discount",
                DiscountRate = 80,
                ApplyScope = "category",
                ApplyScopeId = categoryId,
                StartTime = now.AddDays(-1),
                EndTime = now.AddDays(1),
                Status = "inactive",
                CreatedAt = now,
                UpdatedAt = now
            }
        };
        var orders = new List<Order>();
        var visits = new List<CustomerStoreVisit>();

        var db = new Mock<ByteBiteDbContext>(new DbContextOptionsBuilder<ByteBiteDbContext>().Options);
        db.Setup(context => context.Stores).Returns(CreateDbSet([store]).Object);
        db.Setup(context => context.Products).Returns(CreateDbSet([product]).Object);
        db.Setup(context => context.DiscountRules).Returns(CreateDbSet(discountRules).Object);
        db.Setup(context => context.Orders).Returns(CreateDbSet(orders).Object);
        db.Setup(context => context.CustomerStoreVisits).Returns(CreateDbSet(visits).Object);
        db.Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new OrderService(db.Object);

        var order = await service.CreateOrderAsync(
            storeId,
            customerId: null,
            deviceId: "device-discount-test",
            diningMode: "dine_in",
            items: [new CreateOrderItemInput { ProductId = productId, Quantity = 20 }],
            remark: null);

        order.TotalAmount.Should().Be(400);
        order.DiscountAmount.Should().Be(10);
        order.ActualAmount.Should().Be(390);
        order.DiscountRuleId.Should().Be(fullReductionRuleId);
        discountRules[0].UsedCount.Should().Be(1);
        orders.Should().ContainSingle();
    }

    private static Mock<DbSet<T>> CreateDbSet<T>(List<T> data) where T : class
    {
        var queryable = (IQueryable<T>)new TestAsyncEnumerable<T>(data);
        var mock = new Mock<DbSet<T>>();

        mock.As<IAsyncEnumerable<T>>()
            .Setup(set => set.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(() => new TestAsyncEnumerator<T>(data.GetEnumerator()));
        mock.As<IQueryable<T>>().Setup(set => set.Provider).Returns(queryable.Provider);
        mock.As<IQueryable<T>>().Setup(set => set.Expression).Returns(queryable.Expression);
        mock.As<IQueryable<T>>().Setup(set => set.ElementType).Returns(queryable.ElementType);
        mock.As<IQueryable<T>>().Setup(set => set.GetEnumerator()).Returns(() => data.GetEnumerator());
        mock.Setup(set => set.Add(It.IsAny<T>()))
            .Callback<T>(data.Add)
            .Returns((EntityEntry<T>)null!);

        return mock;
    }

    private sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        public TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

        public IQueryable CreateQuery(Expression expression)
            => new TestAsyncEnumerable<TEntity>(StripIncludes(expression));

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            => new TestAsyncEnumerable<TElement>(StripIncludes(expression));

        public object? Execute(Expression expression)
            => _inner.Execute(StripIncludes(expression));

        public TResult Execute<TResult>(Expression expression)
            => _inner.Execute<TResult>(StripIncludes(expression));

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var result = typeof(IQueryProvider)
                .GetMethods()
                .Single(method => method.Name == nameof(IQueryProvider.Execute)
                    && method.IsGenericMethodDefinition
                    && method.GetParameters().Length == 1)
                .MakeGenericMethod(resultType)
                .Invoke(this, [StripIncludes(expression)]);

            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, [result])!;
        }

        private static Expression StripIncludes(Expression expression)
            => new IncludeStripper().Visit(expression)!;
    }

    private sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }

        public TestAsyncEnumerable(Expression expression) : base(expression) { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    private sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    }

    private sealed class IncludeStripper : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                    || node.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude)))
            {
                return Visit(node.Arguments[0])!;
            }

            return base.VisitMethodCall(node);
        }
    }
}
