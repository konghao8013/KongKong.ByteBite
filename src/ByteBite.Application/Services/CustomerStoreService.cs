using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 顾客店铺服务 - 顾客端扫码进入店铺后获取菜单数据（店铺+分类+商品）
/// </summary>
public class CustomerStoreService
{
    private readonly ByteBiteDbContext _db;

    public CustomerStoreService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 获取店铺菜单 - 返回店铺信息、可见分类、上架商品（含规格）
    /// </summary>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>包含Store、Categories、Products的匿名对象</returns>
    /// <exception cref="BusinessException">404-店铺不存在</exception>
    public async Task<object> GetStoreMenuAsync(Guid storeId, CancellationToken ct = default)
    {
        var store = await _db.Stores.FindAsync([storeId], ct) ?? throw new BusinessException(404, "店铺不存在");
        var categories = await _db.Categories.Where(c => c.StoreId == storeId && c.IsVisible).OrderBy(c => c.SortOrder).ToListAsync(ct);
        // 只返回上架状态的商品，含规格组和规格选项
        var products = await _db.Products.Include(p => p.SpecGroups).ThenInclude(sg => sg.SpecOptions).Where(p => p.StoreId == storeId && p.Status == "on").OrderBy(p => p.SortOrder).ToListAsync(ct);
        return new { Store = store, Categories = categories, Products = products };
    }
}