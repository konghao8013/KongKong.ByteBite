using System.Collections;
using System.Linq.Expressions;
using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;

namespace ByteBite.UnitTests.Current;

public class ConversationServiceTests
{
    [Fact]
    public async Task SendMessageAsync_TracksUnreadCountsForCustomerAndMerchant()
    {
        var fixture = ConversationFixture.CreateWithConversation();
        var service = new ConversationService(fixture.Db.Object);

        var customerMessage = await service.SendMessageAsync(
            fixture.ConversationId,
            "customer",
            fixture.CustomerId,
            "请尽快出餐");

        customerMessage.SenderType.Should().Be("customer");
        customerMessage.Content.Should().Be("请尽快出餐");
        (await service.GetUnreadCountForStoreAsync(fixture.StoreId)).Should().Be(1);
        (await service.GetUnreadCountForCustomerAsync(fixture.CustomerId, fixture.DeviceId)).Should().Be(0);

        var merchantMessage = await service.SendMessageAsync(
            fixture.ConversationId,
            "merchant",
            fixture.MerchantId,
            "已经开始制作");

        merchantMessage.SenderType.Should().Be("merchant");
        (await service.GetUnreadCountForStoreAsync(fixture.StoreId)).Should().Be(1);
        (await service.GetUnreadCountForCustomerAsync(fixture.CustomerId, fixture.DeviceId)).Should().Be(1);

        var messages = await service.GetMessagesAsync(fixture.ConversationId);
        messages.Select(message => message.Content).Should().Equal("请尽快出餐", "已经开始制作");
        fixture.Conversation.LastMessageAt.Should().Be(merchantMessage.CreatedAt);
    }

    [Fact]
    public async Task MarkReadAsync_ClearsUnreadCountForRequestedSideOnly()
    {
        var fixture = ConversationFixture.CreateWithConversation();
        fixture.Conversation.CustomerUnreadCount = 2;
        fixture.Conversation.MerchantUnreadCount = 3;
        var service = new ConversationService(fixture.Db.Object);

        await service.MarkReadAsync(fixture.ConversationId, "merchant");

        fixture.Conversation.MerchantUnreadCount.Should().Be(0);
        fixture.Conversation.CustomerUnreadCount.Should().Be(2);

        await service.MarkReadAsync(fixture.ConversationId, "customer");

        fixture.Conversation.CustomerUnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task GetOrCreateByOrderAsync_UsesCachedAnonymousCustomerIdForDeviceOrder()
    {
        var fixture = ConversationFixture.CreateWithoutConversation();
        var service = new ConversationService(fixture.Db.Object);

        var conversation = await service.GetOrCreateByOrderAsync(
            fixture.OrderId,
            fixture.CustomerId,
            fixture.DeviceId);

        conversation.OrderId.Should().Be(fixture.OrderId);
        conversation.StoreId.Should().Be(fixture.StoreId);
        conversation.CustomerId.Should().Be(fixture.CustomerId);
        conversation.DeviceId.Should().Be(fixture.DeviceId);
        fixture.Conversations.Should().ContainSingle();
    }

    [Fact]
    public async Task GetByCustomerAsync_DoesNotMatchNullDeviceIdWhenCustomerIdExists()
    {
        var fixture = ConversationFixture.CreateWithConversation();
        fixture.Conversation.CustomerUnreadCount = 1;
        fixture.AddConversation(customerId: Guid.NewGuid(), deviceId: null, customerUnreadCount: 5);
        var service = new ConversationService(fixture.Db.Object);

        var conversations = await service.GetByCustomerAsync(fixture.CustomerId, null);
        var unreadCount = await service.GetUnreadCountForCustomerAsync(fixture.CustomerId, null);

        conversations.Should().ContainSingle(conversation => conversation.Id == fixture.ConversationId);
        unreadCount.Should().Be(1);
    }

    private sealed class ConversationFixture
    {
        public Guid StoreId { get; } = Guid.NewGuid();
        public Guid CustomerId { get; } = Guid.NewGuid();
        public Guid MerchantId { get; } = Guid.NewGuid();
        public Guid OrderId { get; } = Guid.NewGuid();
        public Guid ConversationId { get; } = Guid.NewGuid();
        public string DeviceId { get; } = "dev_message_test";
        public List<Conversation> Conversations { get; } = [];
        public Conversation Conversation { get; private set; } = null!;
        public Mock<ByteBiteDbContext> Db { get; private set; } = null!;

        public static ConversationFixture CreateWithConversation()
        {
            var fixture = CreateWithoutConversation();
            fixture.Conversation = new Conversation
            {
                Id = fixture.ConversationId,
                OrderId = fixture.OrderId,
                StoreId = fixture.StoreId,
                CustomerId = fixture.CustomerId,
                DeviceId = fixture.DeviceId,
                Order = fixture.BuildOrder(customerId: fixture.CustomerId),
                Store = fixture.BuildStore(),
                Customer = fixture.BuildCustomer(),
                LastMessageAt = DateTime.UtcNow.AddMinutes(-5),
                CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-5)
            };
            fixture.Conversations.Add(fixture.Conversation);
            return fixture.ConfigureDb([]);
        }

        public static ConversationFixture CreateWithoutConversation()
        {
            var fixture = new ConversationFixture();
            return fixture.ConfigureDb([]);
        }

        private ConversationFixture ConfigureDb(List<ConversationMessage> messages)
        {
            var stores = new List<Store> { BuildStore() };
            var customers = new List<Customer> { BuildCustomer() };
            var orders = new List<Order> { BuildOrder(customerId: null) };

            Db = new Mock<ByteBiteDbContext>(new DbContextOptionsBuilder<ByteBiteDbContext>().Options);
            Db.Setup(context => context.Stores).Returns(CreateDbSet(stores).Object);
            Db.Setup(context => context.Customers).Returns(CreateDbSet(customers).Object);
            Db.Setup(context => context.Orders).Returns(CreateDbSet(orders).Object);
            Db.Setup(context => context.Conversations).Returns(CreateDbSet(Conversations).Object);
            Db.Setup(context => context.ConversationMessages).Returns(CreateDbSet(messages).Object);
            Db.Setup(context => context.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            return this;
        }

        private Store BuildStore() => new()
        {
            Id = StoreId,
            MerchantId = MerchantId,
            Name = "测试门店",
            StoreCode = "000001",
            BusinessStatus = "open",
            DiningMode = "dine_in,takeaway",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        private Customer BuildCustomer() => new()
        {
            Id = CustomerId,
            DeviceId = DeviceId,
            Nickname = "匿名顾客",
            IsRegistered = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        private Order BuildOrder(Guid? customerId) => new()
        {
            Id = OrderId,
            StoreId = StoreId,
            CustomerId = customerId,
            DeviceId = DeviceId,
            OrderNo = "202605280001",
            PickupCodeValue = 1,
            DiningMode = "dine_in",
            Status = "pending",
            TotalAmount = 20,
            ActualAmount = 20,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Store = BuildStore()
        };

        public void AddConversation(Guid customerId, string? deviceId, int customerUnreadCount)
        {
            var now = DateTime.UtcNow;
            var orderId = Guid.NewGuid();
            var order = BuildOrder(customerId);
            order.Id = orderId;
            var customer = BuildCustomer();
            customer.Id = customerId;
            Conversations.Add(new Conversation
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                StoreId = StoreId,
                CustomerId = customerId,
                DeviceId = deviceId,
                CustomerUnreadCount = customerUnreadCount,
                MerchantUnreadCount = 0,
                Order = order,
                Store = BuildStore(),
                Customer = customer,
                LastMessageAt = now,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
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
