using ByteBite.Application.DTOs.Customer;
using ByteBite.Application.DTOs.Order;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 订单服务单元测试 - 覆盖订单创建验证、金额计算、状态流转
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IStoreRepository> _storeRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IDiscountRuleRepository> _discountRepoMock;
    private readonly Mock<IOrderNotificationService> _notificationMock;
    private readonly OrderService _service;

    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();

    public OrderServiceTests()
    {
        _orderRepoMock = new Mock<IOrderRepository>();
        _storeRepoMock = new Mock<IStoreRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _discountRepoMock = new Mock<IDiscountRuleRepository>();
        _notificationMock = new Mock<IOrderNotificationService>();

        _service = new OrderService(
            _orderRepoMock.Object,
            _storeRepoMock.Object,
            _productRepoMock.Object,
            _discountRepoMock.Object,
            _notificationMock.Object);
    }

    /// <summary>
    /// 创建营业中的店铺实体
    /// </summary>
    private StoreEntity CreateOpenStore(decimal packingFee = 0m)
    {
        return new StoreEntity
        {
            Id = _storeId,
            MerchantId = Guid.NewGuid(),
            Name = "测试店铺",
            BusinessStatus = "open",
            PackingFee = packingFee,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建已上架的商品实体
    /// </summary>
    private ProductEntity CreateOnProduct(
        Guid? storeId = null,
        decimal basePrice = 10m,
        int minOrderQty = 1,
        string status = "on")
    {
        return new ProductEntity
        {
            Id = _productId,
            StoreId = storeId ?? _storeId,
            CategoryId = Guid.NewGuid(),
            Name = "测试商品",
            BasePrice = basePrice,
            Status = status,
            MinOrderQty = minOrderQty,
            SpecGroups = [],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建默认的下单请求
    /// </summary>
    private CreateOrderRequest CreateOrderRequest(
        string diningMode = "dine_in",
        int quantity = 1,
        Guid? discountRuleId = null)
    {
        return new CreateOrderRequest
        {
            StoreId = _storeId,
            DiningMode = diningMode,
            Items =
            [
                new OrderItemRequest
                {
                    ProductId = _productId,
                    Quantity = quantity,
                    SelectedSpecOptionIds = []
                }
            ],
            DiscountRuleId = discountRuleId
        };
    }

    /// <summary>
    /// 设置创建订单的通用Mock（店铺营业+商品上架+取货码不重复）
    /// </summary>
    private void SetupCommonCreateMocks(
        StoreEntity? store = null,
        ProductEntity? product = null,
        decimal packingFee = 0m)
    {
        store ??= CreateOpenStore(packingFee);
        product ??= CreateOnProduct();

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _orderRepoMock
            .Setup(r => r.PickupCodeExistsAsync(It.IsAny<string>(), _storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _orderRepoMock
            .Setup(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _orderRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notificationMock
            .Setup(n => n.SendNewOrderNotificationAsync(It.IsAny<Guid>(), It.IsAny<OrderDto>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// 创建指定状态的订单实体
    /// </summary>
    private OrderEntity CreateOrderWithStatus(string status)
    {
        return new OrderEntity
        {
            Id = Guid.NewGuid(),
            OrderNo = "ORD20260101000001",
            StoreId = _storeId,
            PickupCode = "A001",
            DiningMode = "dine_in",
            Status = status,
            TotalAmount = 10m,
            ActualAmount = 10m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            OrderItems = []
        };
    }

    /// <summary>
    /// 设置状态流转测试的通用Mock
    /// </summary>
    private void SetupStatusTransitionMocks(OrderEntity order)
    {
        _orderRepoMock
            .Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _orderRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateOpenStore());

        _notificationMock
            .Setup(n => n.SendOrderStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notificationMock
            .Setup(n => n.SendOrderCancelledAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task CreateOrderAsync_StoreNotFound_ThrowsInvalidOperationException()
    {
        // 店铺不存在时应抛出异常
        _storeRepoMock
            .Setup(r => r.GetByIdAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreEntity?)null);

        var request = CreateOrderRequest();

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("店铺不存在");
    }

    [Fact]
    public async Task CreateOrderAsync_StoreClosed_ThrowsInvalidOperationException()
    {
        // 店铺休息中时应抛出异常
        var store = CreateOpenStore();
        store.BusinessStatus = "closed";

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        var request = CreateOrderRequest();

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("店铺休息中，暂不可下单");
    }

    [Fact]
    public async Task CreateOrderAsync_ProductNotFound_ThrowsInvalidOperationException()
    {
        // 商品不存在时应抛出异常
        SetupCommonCreateMocks(product: null);

        _productRepoMock
            .Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        var request = CreateOrderRequest();

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"商品不存在：{_productId}");
    }

    [Fact]
    public async Task CreateOrderAsync_ProductNotBelongToStore_ThrowsInvalidOperationException()
    {
        // 商品不属于该店铺时应抛出异常
        var product = CreateOnProduct(storeId: Guid.NewGuid());

        SetupCommonCreateMocks(product: product);

        var request = CreateOrderRequest();

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"商品不属于该店铺：{_productId}");
    }

    [Fact]
    public async Task CreateOrderAsync_ProductOffShelf_ThrowsInvalidOperationException()
    {
        // 商品已下架时应抛出异常
        var product = CreateOnProduct(status: "off");

        SetupCommonCreateMocks(product: product);

        var request = CreateOrderRequest();

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("商品已下架：测试商品");
    }

    [Fact]
    public async Task CreateOrderAsync_QuantityBelowMinOrderQty_ThrowsInvalidOperationException()
    {
        // 数量低于最低起购数时应抛出异常
        var product = CreateOnProduct(minOrderQty: 2);

        SetupCommonCreateMocks(product: product);

        var request = CreateOrderRequest(quantity: 1);

        var act = () => _service.CreateOrderAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("商品「测试商品」最低起购2份");
    }

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_ReturnsOrderDto()
    {
        // 有效订单应返回正确的OrderDto
        SetupCommonCreateMocks();

        var request = CreateOrderRequest();

        var result = await _service.CreateOrderAsync(request);

        result.Should().NotBeNull();
        result.Status.Should().Be("pending");
        result.TotalAmount.Should().BeGreaterThan(0);
        result.StoreId.Should().Be(_storeId);

        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<OrderEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _orderRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_WithDiscount_AppliesDiscount()
    {
        // 有效优惠活动应减免金额
        var discountId = Guid.NewGuid();
        var discount = new DiscountRuleEntity
        {
            Id = discountId,
            StoreId = _storeId,
            Name = "满10减5",
            DiscountType = "full_reduction",
            ThresholdAmount = 10m,
            DiscountAmount = 5m,
            ApplyScope = "all",
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(1),
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        SetupCommonCreateMocks();

        _discountRepoMock
            .Setup(r => r.GetByIdAsync(discountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(discount);

        var request = CreateOrderRequest(discountRuleId: discountId);

        var result = await _service.CreateOrderAsync(request);

        result.DiscountAmount.Should().BeGreaterThan(0);
        result.ActualAmount.Should().BeLessThan(result.TotalAmount);
    }

    [Fact]
    public async Task CreateOrderAsync_TakeawayMode_IncludesPackingFee()
    {
        // 外卖模式应包含打包费
        SetupCommonCreateMocks(packingFee: 2m, store: CreateOpenStore(2m));

        var request = CreateOrderRequest(diningMode: "takeaway");

        var result = await _service.CreateOrderAsync(request);

        result.PackingFee.Should().Be(2m);
    }

    [Fact]
    public async Task AcceptOrderAsync_PendingOrder_ChangesToAccepted()
    {
        // 待接单订单接单后状态变为已接单
        var order = CreateOrderWithStatus("pending");
        SetupStatusTransitionMocks(order);

        var result = await _service.AcceptOrderAsync(order.Id);

        result.Status.Should().Be("accepted");
    }

    [Fact]
    public async Task AcceptOrderAsync_NonPendingOrder_ThrowsInvalidOperationException()
    {
        // 非待接单状态的订单接单时应抛出异常
        var order = CreateOrderWithStatus("accepted");
        SetupStatusTransitionMocks(order);

        var act = () => _service.AcceptOrderAsync(order.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("仅待接单状态可接单");
    }

    [Fact]
    public async Task RejectOrderAsync_PendingOrder_ChangesToCancelled()
    {
        // 待接单订单拒单后状态变为已取消，且设置拒单原因
        var order = CreateOrderWithStatus("pending");
        SetupStatusTransitionMocks(order);

        var result = await _service.RejectOrderAsync(order.Id, "库存不足");

        result.Status.Should().Be("cancelled");
        order.RejectReason.Should().Be("库存不足");
    }

    [Fact]
    public async Task StartPreparingAsync_AcceptedOrder_ChangesToPreparing()
    {
        // 已接单订单开始制作后状态变为制作中
        var order = CreateOrderWithStatus("accepted");
        SetupStatusTransitionMocks(order);

        var result = await _service.StartPreparingAsync(order.Id);

        result.Status.Should().Be("preparing");
    }

    [Fact]
    public async Task MarkReadyAsync_PreparingOrder_ChangesToReady()
    {
        // 制作中订单备餐完毕后状态变为待取餐
        var order = CreateOrderWithStatus("preparing");
        SetupStatusTransitionMocks(order);

        var result = await _service.MarkReadyAsync(order.Id);

        result.Status.Should().Be("ready");
    }

    [Fact]
    public async Task CompleteOrderAsync_ReadyOrder_ChangesToCompleted()
    {
        // 待取餐订单完成后状态变为已完成
        var order = CreateOrderWithStatus("ready");
        SetupStatusTransitionMocks(order);

        var result = await _service.CompleteOrderAsync(order.Id);

        result.Status.Should().Be("completed");
    }

    [Fact]
    public async Task CancelOrderAsync_PendingOrder_ChangesToCancelled()
    {
        // 待接单订单取消后状态变为已取消
        var order = CreateOrderWithStatus("pending");
        SetupStatusTransitionMocks(order);

        var result = await _service.CancelOrderAsync(order.Id);

        result.Status.Should().Be("cancelled");
    }

    [Fact]
    public async Task CancelOrderAsync_NonPendingOrder_ThrowsInvalidOperationException()
    {
        // 非待接单状态的订单取消时应抛出异常
        var order = CreateOrderWithStatus("accepted");
        SetupStatusTransitionMocks(order);

        var act = () => _service.CancelOrderAsync(order.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("仅待接单状态可取消订单");
    }
}
