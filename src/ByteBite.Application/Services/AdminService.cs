using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 管理员服务 - 处理管理员登录、商家管理、审核日志等业务逻辑
/// </summary>
public class AdminService
{
    private readonly ByteBiteDbContext _db;

    public AdminService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 管理员登录 - 验证用户名和密码，生成Token返回
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="password">明文密码</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>登录成功的管理员实体（含Token）</returns>
    /// <exception cref="BusinessException">401-用户名或密码错误, 403-账号已被禁用</exception>
    public async Task<Admin> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Username == username, ct);
        if (admin == null || !PasswordHasher.VerifyPassword(password, admin.PasswordHash))
            throw new BusinessException(401, "用户名或密码错误");
        if (admin.Status != "active") throw new BusinessException(403, "账号已被禁用");
        admin.LastLoginAt = DateTime.UtcNow;
        // 生成Base64编码的Token用于前端鉴权
        admin.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        await _db.SaveChangesAsync(ct);
        return admin;
    }

    /// <summary>
    /// 获取商家列表 - 支持按状态和关键词筛选
    /// </summary>
    /// <param name="status">商家状态筛选（可选）</param>
    /// <param name="keyword">手机号或昵称关键词（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>商家列表，按创建时间倒序</returns>
    public async Task<List<Merchant>> GetMerchantsAsync(string? status, string? keyword, CancellationToken ct = default)
    {
        var query = _db.Merchants.AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(m => m.Status == status);
        if (!string.IsNullOrEmpty(keyword)) query = query.Where(m => m.Phone.Contains(keyword) || (m.Nickname != null && m.Nickname.Contains(keyword)));
        return await query.OrderByDescending(m => m.CreatedAt).ToListAsync(ct);
    }

    /// <summary>
    /// 更新商家状态 - 管理员审核/禁用/激活商家，同时记录审计日志
    /// </summary>
    /// <param name="merchantId">商家ID</param>
    /// <param name="status">新状态</param>
    /// <param name="operatorId">操作管理员ID</param>
    /// <param name="reason">操作原因</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的商家实体</returns>
    /// <exception cref="BusinessException">404-商家不存在</exception>
    public async Task<Merchant> UpdateMerchantStatusAsync(Guid merchantId, string status, Guid operatorId, string reason, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.FindAsync([merchantId], ct) ?? throw new BusinessException(404, "商家不存在");
        var oldStatus = merchant.Status;
        merchant.Status = status; merchant.UpdatedAt = DateTime.UtcNow;
        try
        {
            // 记录状态变更审计日志
            _db.MerchantAuditLogs.Add(new MerchantAuditLog
            {
                Id = Guid.NewGuid(), MerchantId = merchantId, AdminId = operatorId, Action = $"status_change:{oldStatus}->{status}",
                Reason = reason, PreviousStatus = oldStatus, NewStatus = status,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // 审计日志写入失败时回滚该条记录，确保商家状态更新不受影响
            var entry = _db.ChangeTracker.Entries<MerchantAuditLog>().FirstOrDefault(e => e.State == EntityState.Added);
            if (entry != null) _db.MerchantAuditLogs.Remove(entry.Entity);
            await _db.SaveChangesAsync(ct);
        }
        return merchant;
    }

    /// <summary>
    /// 更新管理员信息 - 修改显示名称或角色
    /// </summary>
    /// <param name="adminId">管理员ID</param>
    /// <param name="displayName">显示名称（可选）</param>
    /// <param name="role">角色（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>更新后的管理员实体</returns>
    /// <exception cref="BusinessException">404-管理员不存在</exception>
    public async Task<Admin> UpdateAdminAsync(Guid adminId, string? displayName, string? role, CancellationToken ct = default)
    {
        var admin = await _db.Admins.FindAsync([adminId], ct) ?? throw new BusinessException(404, "管理员不存在");
        if (displayName != null) admin.DisplayName = displayName;
        if (role != null) admin.Role = role;
        admin.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return admin;
    }

    /// <summary>
    /// 获取商家审计日志 - 按商家ID筛选，最近100条
    /// </summary>
    /// <param name="merchantId">商家ID（可选，为空则查全部）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>审计日志列表</returns>
    public async Task<List<MerchantAuditLog>> GetMerchantAuditLogsAsync(Guid? merchantId, CancellationToken ct = default)
    {
        var query = _db.MerchantAuditLogs.AsQueryable();
        if (merchantId != null) query = query.Where(l => l.MerchantId == merchantId);
        return await query.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync(ct);
    }

    /// <summary>
    /// 获取管理员操作日志 - 按管理员ID筛选，最近100条
    /// </summary>
    /// <param name="adminId">管理员ID（可选，为空则查全部）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>操作日志列表</returns>
    public async Task<List<AdminOperationLog>> GetAdminOperationLogsAsync(Guid? adminId, CancellationToken ct = default)
    {
        var query = _db.AdminOperationLogs.AsQueryable();
        if (adminId != null) query = query.Where(l => l.AdminId == adminId);
        return await query.OrderByDescending(l => l.CreatedAt).Take(100).ToListAsync(ct);
    }

    /// <summary>
    /// 管理员退出登录 - 清除Token使当前登录凭证失效
    /// </summary>
    /// <param name="adminId">管理员ID</param>
    /// <param name="ct">取消令牌</param>
    public async Task LogoutAsync(Guid adminId, CancellationToken ct = default)
    {
        var admin = await _db.Admins.FindAsync([adminId], ct);
        if (admin != null) { admin.Token = null; await _db.SaveChangesAsync(ct); }
    }

    /// <summary>
    /// 获取平台统计数据 - 商家数、店铺数、订单数、营收总额等
    /// </summary>
    /// <param name="ct">取消令牌</param>
    /// <returns>平台统计概览数据</returns>
    public async Task<object> GetPlatformStatsAsync(CancellationToken ct = default)
    {
        var totalMerchants = await _db.Merchants.CountAsync(ct);
        var activeMerchants = await _db.Merchants.CountAsync(m => m.Status == "active", ct);
        var pendingMerchants = await _db.Merchants.CountAsync(m => m.Status == "pending", ct);
        var totalStores = await _db.Stores.CountAsync(ct);
        var openStores = await _db.Stores.CountAsync(s => s.BusinessStatus == "open", ct);
        var totalOrders = await _db.Orders.CountAsync(ct);
        var completedOrders = await _db.Orders.CountAsync(o => o.Status == "completed", ct);
        var totalRevenue = await _db.Orders.Where(o => o.Status == "completed").SumAsync(o => o.ActualAmount, ct);
        var today = DateTime.UtcNow.Date;
        var todayOrders = await _db.Orders.CountAsync(o => o.CreatedAt.Date == today, ct);
        var todayRevenue = await _db.Orders.Where(o => o.Status == "completed" && o.CompletedAt.HasValue && o.CompletedAt.Value.Date == today).SumAsync(o => o.ActualAmount, ct);
        var totalProducts = await _db.Products.CountAsync(ct);
        var totalCategories = await _db.Categories.CountAsync(ct);
        return new
        {
            TotalMerchants = totalMerchants, ActiveMerchants = activeMerchants, PendingMerchants = pendingMerchants,
            TotalStores = totalStores, OpenStores = openStores,
            TotalOrders = totalOrders, CompletedOrders = completedOrders, TotalRevenue = totalRevenue,
            TodayOrders = todayOrders, TodayRevenue = todayRevenue,
            TotalProducts = totalProducts, TotalCategories = totalCategories
        };
    }
}
