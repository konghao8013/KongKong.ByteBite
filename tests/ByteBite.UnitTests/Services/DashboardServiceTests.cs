using ByteBite.Application.DTOs.Dashboard;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 经营数据看板服务单元测试 - 覆盖概览统计、商品销售、营收趋势、时段分布
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<IDashboardRepository> _repoMock;
    private readonly DashboardService _service;

    private readonly Guid _storeId = Guid.NewGuid();

    public DashboardServiceTests()
    {
        _repoMock = new Mock<IDashboardRepository>();
        _service = new DashboardService(_repoMock.Object);
    }

    /// <summary>
    /// 创建今日经营统计实体
    /// </summary>
    private DailyStoreStatEntity CreateTodayStats()
    {
        return new DailyStoreStatEntity
        {
            Id = Guid.NewGuid(),
            StoreId = _storeId,
            StatDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalOrders = 50,
            CompletedOrders = 45,
            CancelledOrders = 2,
            TotalRevenue = 3000m,
            ActualRevenue = 2800m,
            DiscountAmount = 200m,
            PackingFee = 100m,
            AvgOrderAmount = 56m,
            NewCustomers = 10,
            ReturningCustomers = 35,
            PeakHour = 12,
            PeakHourOrders = 15,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建昨日经营统计实体
    /// </summary>
    private DailyStoreStatEntity CreateYesterdayStats()
    {
        return new DailyStoreStatEntity
        {
            Id = Guid.NewGuid(),
            StoreId = _storeId,
            StatDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
            TotalOrders = 40,
            ActualRevenue = 2500m,
            AvgOrderAmount = 62.5m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建商品销售统计实体
    /// </summary>
    private ProductSalesStatEntity CreateProductSalesStat()
    {
        return new ProductSalesStatEntity
        {
            Id = Guid.NewGuid(),
            StoreId = _storeId,
            ProductId = Guid.NewGuid(),
            ProductName = "拿铁咖啡",
            CategoryId = Guid.NewGuid(),
            CategoryName = "咖啡",
            StatDate = DateOnly.FromDateTime(DateTime.UtcNow),
            SalesQuantity = 30,
            SalesAmount = 600m,
            OrderCount = 25,
            CancelCount = 1,
            AvgUnitPrice = 20m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建时段统计实体
    /// </summary>
    private HourlyStatEntity CreateHourlyStat(int hour, int orderCount, decimal revenue)
    {
        return new HourlyStatEntity
        {
            Id = Guid.NewGuid(),
            StoreId = _storeId,
            StatDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Hour = hour,
            OrderCount = orderCount,
            Revenue = revenue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task GetOverviewAsync_ReturnsOverviewWithTodayStats()
    {
        // 有今日数据时应返回正确的概览统计
        var todayStats = new List<DailyStoreStatEntity> { CreateTodayStats() };
        var yesterdayStats = CreateYesterdayStats();

        _repoMock
            .Setup(r => r.GetDailyStatsAsync(_storeId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todayStats);
        _repoMock
            .Setup(r => r.GetYesterdayStatsAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(yesterdayStats);
        _repoMock
            .Setup(r => r.GetPendingOrderCountAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var result = await _service.GetOverviewAsync(_storeId);

        result.TodayOrders.Should().Be(50);
        result.TodayRevenue.Should().Be(2800m);
        result.PendingOrders.Should().Be(3);
    }

    [Fact]
    public async Task GetOverviewAsync_NoTodayStats_ReturnsZeroValues()
    {
        // 无今日数据时应默认返回0
        _repoMock
            .Setup(r => r.GetDailyStatsAsync(_storeId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _repoMock
            .Setup(r => r.GetYesterdayStatsAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyStoreStatEntity?)null);
        _repoMock
            .Setup(r => r.GetPendingOrderCountAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _service.GetOverviewAsync(_storeId);

        result.TodayOrders.Should().Be(0);
        result.TodayRevenue.Should().Be(0m);
        result.PendingOrders.Should().Be(0);
    }

    [Fact]
    public async Task GetProductSalesAsync_ReturnsProductSalesList()
    {
        // 商品销售统计应正确映射返回
        var stats = new List<ProductSalesStatEntity> { CreateProductSalesStat() };

        _repoMock
            .Setup(r => r.GetProductSalesAsync(_storeId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var query = new DashboardQueryDto { StoreId = _storeId, TimeRange = "today" };

        var result = await _service.GetProductSalesAsync(query);

        result.Should().HaveCount(1);
        result[0].ProductName.Should().Be("拿铁咖啡");
        result[0].SalesQuantity.Should().Be(30);
        result[0].SalesAmount.Should().Be(600m);
    }

    [Fact]
    public async Task GetRevenueTrendAsync_ReturnsTrendData()
    {
        // 营收趋势应按日期排序并正确映射
        var day1 = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var day2 = DateOnly.FromDateTime(DateTime.UtcNow);

        var stats = new List<DailyStoreStatEntity>
        {
            new()
            {
                Id = Guid.NewGuid(), StoreId = _storeId, StatDate = day2,
                ActualRevenue = 2800m, TotalOrders = 50, AvgOrderAmount = 56m,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(), StoreId = _storeId, StatDate = day1,
                ActualRevenue = 2500m, TotalOrders = 40, AvgOrderAmount = 62.5m,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            }
        };

        _repoMock
            .Setup(r => r.GetDailyStatsAsync(_storeId, It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(stats);

        var query = new DashboardQueryDto { StoreId = _storeId, TimeRange = "last7days" };

        var result = await _service.GetRevenueTrendAsync(query);

        result.Should().HaveCount(2);
        result[0].Date.Should().Be(day1);
        result[0].Value.Should().Be(2500m);
        result[1].Date.Should().Be(day2);
        result[1].Value.Should().Be(2800m);
    }

    [Fact]
    public async Task GetHourlyDistributionAsync_ReturnsHourlyData()
    {
        // 时段分布应按小时排序并正确映射
        var hourlyStats = new List<HourlyStatEntity>
        {
            CreateHourlyStat(12, 15, 900m),
            CreateHourlyStat(11, 8, 480m),
            CreateHourlyStat(13, 10, 600m)
        };

        _repoMock
            .Setup(r => r.GetHourlyStatsAsync(_storeId, It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(hourlyStats);

        var result = await _service.GetHourlyDistributionAsync(_storeId, DateOnly.FromDateTime(DateTime.UtcNow));

        result.Should().HaveCount(3);
        result[0].Hour.Should().Be(11);
        result[1].Hour.Should().Be(12);
        result[2].Hour.Should().Be(13);
        result[1].OrderCount.Should().Be(15);
        result[1].Revenue.Should().Be(900m);
    }
}
