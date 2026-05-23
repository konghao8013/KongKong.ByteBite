using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 模板服务 - 处理行业模板的查询和应用，模板包含预设分类和商品供商家一键使用
/// </summary>
public class TemplateService
{
    private readonly ByteBiteDbContext _db;

    public TemplateService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 根据ID获取模板详情 - 包含模板分类、模板商品、规格组和规格选项
    /// </summary>
    /// <param name="id">模板ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>模板实体（含分类和商品）</returns>
    /// <exception cref="BusinessException">404-模板不存在</exception>
    public async Task<StoreTemplate> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.StoreTemplates.Include(t => t.TemplateProducts).ThenInclude(p => p.TemplateSpecGroups).ThenInclude(sg => sg.TemplateSpecOptions).Include(t => t.TemplateCategories).FirstOrDefaultAsync(t => t.Id == id, ct) ?? throw new BusinessException(404, "模板不存在");

    /// <summary>
    /// 获取模板列表 - 支持按行业分类筛选，按使用次数倒序
    /// </summary>
    /// <param name="industryCategoryId">行业分类ID（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>活跃状态的模板列表</returns>
    public async Task<List<StoreTemplate>> GetListAsync(Guid? industryCategoryId, CancellationToken ct = default)
    {
        var query = _db.StoreTemplates.Include(t => t.TemplateProducts).ThenInclude(p => p.TemplateSpecGroups).ThenInclude(sg => sg.TemplateSpecOptions).Include(t => t.TemplateCategories).Where(t => t.Status == "active");
        if (industryCategoryId != null) query = query.Where(t => t.IndustryCategoryId == industryCategoryId);
        return await query.OrderByDescending(t => t.UseCount).ToListAsync(ct);
    }

    /// <summary>
    /// 应用模板到店铺 - 增加模板使用计数（实际分类和商品创建需前端配合逐条调用）
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="storeId">店铺ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>模板实体</returns>
    /// <exception cref="BusinessException">404-模板不存在, 404-店铺不存在</exception>
    public async Task<StoreTemplate> ApplyToStoreAsync(Guid templateId, Guid storeId, CancellationToken ct = default)
    {
        var template = await GetByIdAsync(templateId, ct);
        var store = await _db.Stores.FindAsync([storeId], ct) ?? throw new BusinessException(404, "店铺不存在");
        // 增加模板使用计数
        template.UseCount++;
        await _db.SaveChangesAsync(ct);
        return template;
    }
}