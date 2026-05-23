using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 行业分类服务 - 处理行业三级分类的查询，用于模板分类筛选
/// </summary>
public class IndustryCategoryService
{
    private readonly ByteBiteDbContext _db;

    public IndustryCategoryService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 获取所有可见的行业分类 - 按排序权重排列
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>行业分类列表</returns>
    public async Task<List<IndustryCategory>> GetAllAsync(CancellationToken ct = default)
        => await _db.IndustryCategories.Where(i => i.IsVisible).OrderBy(i => i.SortOrder).ToListAsync(ct);

    /// <summary>
    /// 按层级获取行业分类 - 一级/二级/三级分类
    /// </summary>
    /// <param name="level">层级：1/2/3</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>指定层级的行业分类列表</returns>
    public async Task<List<IndustryCategory>> GetByLevelAsync(int level, CancellationToken ct = default)
        => await _db.IndustryCategories.Where(i => i.Level == level && i.IsVisible).OrderBy(i => i.SortOrder).ToListAsync(ct);
}