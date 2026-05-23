# 测试模式与踩坑

本文件按需加载，不是常驻协议。

## Provider 接口模式

业务逻辑中需要用到的静态/环境依赖，必须通过接口包装，便于测试时 Mock。

```csharp
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public interface IGuidProvider
{
    Guid NewGuid { get; }
}

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}

public class SystemGuidProvider : IGuidProvider
{
    public Guid NewGuid => Guid.NewGuid();
}
```

注册：

```csharp
services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
services.AddSingleton<IGuidProvider, SystemGuidProvider>();
```

测试中 Mock：

```csharp
var dateTimeMock = new Mock<IDateTimeProvider>();
dateTimeMock.Setup(p => p.UtcNow).Returns(new DateTime(2026, 5, 20, 12, 0, 0, DateTimeKind.Utc));
```

## Repository Mock 模式

```csharp
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo;
    private readonly Mock<IStoreRepository> _storeRepo;
    private readonly Mock<IDateTimeProvider> _dateTime;
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _orderRepo = new Mock<IOrderRepository>();
        _storeRepo = new Mock<IStoreRepository>();
        _dateTime = new Mock<IDateTimeProvider>();
        _sut = new OrderService(_orderRepo.Object, _storeRepo.Object, _dateTime.Object);
    }
}
```

## WebApplicationFactory 共享基类

```csharp
public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly IServiceScope Scope;

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        var customizedFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ByteBiteDbContext>>();
                services.AddDbContext<ByteBiteDbContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                services.RemoveAll<IDateTimeProvider>();
                services.AddSingleton<TestDateTimeProvider>();
            });
        });

        Client = customizedFactory.CreateClient();
        Scope = customizedFactory.Services.CreateScope();
    }

    protected async Task SeedAsync(Action<ByteBiteDbContext> seedAction)
    {
        var dbContext = Scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        seedAction(dbContext);
        await dbContext.SaveChangesAsync();
    }
}
```

## FluentValidation 测试

```csharp
public class CreateOrderRequestValidatorTests
{
    private readonly CreateOrderRequestValidator _sut = new();

    [Fact]
    public void Validate_EmptyStoreId_Fails()
    {
        var request = new CreateOrderRequest { StoreId = Guid.Empty };

        var result = _sut.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(CreateOrderRequest.StoreId));
    }
}
```

## Mapster 映射测试

```csharp
public class OrderMappingTests
{
    [Fact]
    public void Order_To_OrderDto_MapsCorrectly()
    {
        var order = Order.Create(/* ... */);

        var dto = order.Adapt<OrderDto>();

        dto.Id.Should().Be(order.Id);
        dto.PickupCode.Should().Be(order.PickupCode);
        dto.Status.Should().Be(order.Status.ToString());
    }
}
```

## SignalR Hub 测试

```csharp
public class OrderHubTests
{
    [Fact]
    public async Task SubscribeOrder_JoinsGroup()
    {
        var mockClients = new Mock<IHubClients>();
        var mockGroupManager = new Mock<IGroupManager>();
        var hub = new OrderHub { Groups = mockGroupManager.Object };

        await hub.SubscribeOrder("order-123");

        mockGroupManager.Verify(g => g.AddToGroupAsync(
            hub.Context.ConnectionId, "order-order-123", default), Times.Once);
    }
}
```

## 常见踩坑

| 坑 | 说明 | 解法 |
|---|---|---|
| InMemory 与 PostgreSQL 行为差异 | InMemory 不支持约束、级联删除、原始 SQL | 集成测试用 Testcontainers PostgreSQL，单元测试用 InMemory |
| 测试间状态污染 | 共享 InMemory 数据库导致测试互相影响 | 每个测试用独立数据库 `TestDb_{Guid}` |
| DateTime.Now 不确定性 | 测试中时间不可控 | 使用 `IDateTimeProvider` 接口 |
| 异步测试死锁 | 同步调用异步方法 | 测试方法一律 `async Task`，禁止 `async void` |
| Mock 过度 | Mock 了被测方法本身 | 只 Mock 依赖，不 Mock 被测对象 |
| 集成测试慢 | 每个测试都创建新 Factory | 使用 `IClassFixture` 共享 Factory |
| 覆盖率虚高 | 只测 Happy Path | 必须覆盖异常路径、边界条件、验证失败 |
