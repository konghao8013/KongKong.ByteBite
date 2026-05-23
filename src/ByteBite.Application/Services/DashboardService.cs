using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 数据报表服务 - 处理店铺经营数据概览、分类销售统计、时段分布统计
/// </summary>
public class DashboardService
{
    private readonly ByteBiteDbContext _db;

    public DashboardService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 获取店铺经营概览 - 待处理订单数、昨日订单数、昨日营收
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>包含StoreId、StoreName、PendingOrderCount、YesterdayOrderCount、YesterdayRevenue的匿名对象</returns>
    /// <exception cref="BusinessException">404-店铺不存在</exception>
    public async Task<object> GetOverviewAsync(Guid storeId, CancellationToken ct = default)
    {
        var store = await _db.Stores.FindAsync([storeId], ct) ?? throw new BusinessException(404, "店铺不存在");
        var pendingCount = await _db.Orders.CountAsync(o => o.StoreId == storeId && o.Status == "pending", ct);
        // 统计昨日已完成订单的营收数据
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var yesterdayOrders = await _db.Orders.Where(o => o.StoreId == storeId && o.CompletedAt.HasValue && o.CompletedAt.Value.Date == yesterday.Date).ToListAsync(ct);
        return new { StoreId = storeId, StoreName = store.Name, PendingOrderCount = pendingCount, YesterdayOrderCount = yesterdayOrders.Count, YesterdayRevenue = yesterdayOrders.Sum(o => o.ActualAmount) };
    }

    /// <summary>
    /// 获取分类销售统计 - 指定日期范围内的商品销售数据
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>商品销售统计列表</returns>
    public async Task<List<ProductSalesStat>> GetCategorySalesAsync(Guid storeId, DateOnly startDate, DateOnly endDate, CancellationToken ct = default)
        => await _db.ProductSalesStats.Where(p => p.StoreId == storeId && p.StatDate >= startDate && p.StatDate <= endDate).ToListAsync(ct);

    /// <summary>
    /// 获取时段分布统计 - 指定日期的每小时订单量分布
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="date">统计日期</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>时段分布统计列表</returns>
    public async Task<List<HourlyOrderStat>> GetHourlyDistributionAsync(Guid storeId, DateOnly date, CancellationToken ct = default)
        => await _db.HourlyOrderStats.Where(h => h.StoreId == storeId && h.StatDate == date).ToListAsync(ct);
}