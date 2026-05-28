using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
            if (await _db.Admins.AnyAsync(a => a.Id == operatorId, ct))
            {
                // 记录状态变更审计日志
                _db.MerchantAuditLogs.Add(new MerchantAuditLog
                {
                    Id = Guid.NewGuid(), MerchantId = merchantId, AdminId = operatorId, Action = $"status_change:{oldStatus}->{status}",
                    Reason = reason, PreviousStatus = oldStatus, NewStatus = status,
                    CreatedAt = DateTime.UtcNow
                });
                _db.AdminOperationLogs.Add(new AdminOperationLog
                {
                    Id = Guid.NewGuid(),
                    AdminId = operatorId,
                    Operation = "merchant_status_change",
                    TargetType = "merchant",
                    TargetId = merchantId,
                    Detail = JsonSerializer.Serialize(new { oldStatus, newStatus = status, reason }),
                    CreatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            // 审计日志写入失败时回滚该条记录，确保商家状态更新不受影响
            var entry = _db.ChangeTracker.Entries<MerchantAuditLog>().FirstOrDefault(e => e.State == EntityState.Added);
            if (entry != null) _db.MerchantAuditLogs.Remove(entry.Entity);
            var operationEntry = _db.ChangeTracker.Entries<AdminOperationLog>().FirstOrDefault(e => e.State == EntityState.Added);
            if (operationEntry != null) _db.AdminOperationLogs.Remove(operationEntry.Entity);
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
        var suspendedMerchants = await _db.Merchants.CountAsync(m => m.Status == "suspended", ct);
        var totalStores = await _db.Stores.CountAsync(ct);
        var openStores = await _db.Stores.CountAsync(s => s.BusinessStatus == "open", ct);
        var closedStores = await _db.Stores.CountAsync(s => s.BusinessStatus != "open", ct);
        var totalOrders = await _db.Orders.CountAsync(ct);
        var completedOrders = await _db.Orders.CountAsync(o => o.Status == "completed", ct);
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == "pending", ct);
        var totalRevenue = await _db.Orders.Where(o => o.Status == "completed").SumAsync(o => o.ActualAmount, ct);
        var today = DateTime.UtcNow.Date;
        var todayOrders = await _db.Orders.CountAsync(o => o.CreatedAt.Date == today, ct);
        var todayRevenue = await _db.Orders.Where(o => o.Status == "completed" && o.CompletedAt.HasValue && o.CompletedAt.Value.Date == today).SumAsync(o => o.ActualAmount, ct);
        var totalProducts = await _db.Products.CountAsync(ct);
        var onProducts = await _db.Products.CountAsync(p => p.Status == "on" && p.DeletedAt == null, ct);
        var totalCategories = await _db.Categories.CountAsync(ct);
        var registeredCustomers = await _db.Customers.CountAsync(c => c.IsRegistered, ct);
        var anonymousCustomers = await _db.Customers.CountAsync(c => !c.IsRegistered, ct);
        var activeDiscounts = await _db.DiscountRules.CountAsync(d => d.Status == "active" && d.DeletedAt == null && d.StartTime <= DateTime.UtcNow && d.EndTime >= DateTime.UtcNow, ct);
        var last7DaysOrders = await _db.Orders
            .Where(o => o.CreatedAt >= today.AddDays(-6))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Orders = g.Count(), Revenue = g.Where(o => o.Status == "completed").Sum(o => o.ActualAmount) })
            .OrderBy(x => x.Date)
            .ToListAsync(ct);

        return new
        {
            TotalMerchants = totalMerchants,
            ActiveMerchants = activeMerchants,
            PendingMerchants = pendingMerchants,
            SuspendedMerchants = suspendedMerchants,
            TotalStores = totalStores,
            OpenStores = openStores,
            ClosedStores = closedStores,
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            CompletionRate = totalOrders == 0 ? 0 : Math.Round(completedOrders * 100m / totalOrders, 2),
            TotalRevenue = totalRevenue,
            TodayOrders = todayOrders,
            TodayRevenue = todayRevenue,
            AverageOrderAmount = completedOrders == 0 ? 0 : Math.Round(totalRevenue / completedOrders, 2),
            TotalProducts = totalProducts,
            OnProducts = onProducts,
            TotalCategories = totalCategories,
            RegisteredCustomers = registeredCustomers,
            AnonymousCustomers = anonymousCustomers,
            ActiveDiscounts = activeDiscounts,
            Last7DaysOrders = last7DaysOrders
        };
    }

    public async Task<List<SystemConfig>> GetSystemConfigsAsync(bool publicOnly = false, CancellationToken ct = default)
    {
        await EnsureDefaultConfigsAsync(ct);
        var query = _db.SystemConfigs.AsQueryable();
        if (publicOnly) query = query.Where(c => c.IsPublic);
        return await query.OrderBy(c => c.ConfigKey).ToListAsync(ct);
    }

    public async Task<SystemConfig> UpsertSystemConfigAsync(UpsertSystemConfigInput input, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(input.ConfigKey)) throw new BusinessException(400, "配置键不能为空");
        if (input.ConfigType is not ("string" or "number" or "boolean" or "json")) throw new BusinessException(400, "配置类型不正确");
        ValidateConfigValue(input.ConfigType!, input.ConfigValue ?? string.Empty);

        var now = DateTime.UtcNow;
        var config = await _db.SystemConfigs.FirstOrDefaultAsync(c => c.ConfigKey == input.ConfigKey.Trim(), ct);
        if (config == null)
        {
            config = new SystemConfig
            {
                Id = Guid.NewGuid(),
                ConfigKey = input.ConfigKey.Trim(),
                ConfigValue = input.ConfigValue ?? string.Empty,
                ConfigType = input.ConfigType!,
                Description = input.Description,
                IsPublic = input.IsPublic,
                CreatedAt = now,
                UpdatedAt = now
            };
            _db.SystemConfigs.Add(config);
        }
        else
        {
            config.ConfigValue = input.ConfigValue ?? string.Empty;
            config.ConfigType = input.ConfigType!;
            config.Description = input.Description;
            config.IsPublic = input.IsPublic;
            config.UpdatedAt = now;
        }

        if (input.OperatorId.HasValue)
        {
            _db.AdminOperationLogs.Add(new AdminOperationLog
            {
                Id = Guid.NewGuid(),
                AdminId = input.OperatorId.Value,
                Operation = "config_upsert",
                TargetType = "config",
                TargetId = config.Id,
                Detail = JsonSerializer.Serialize(new { config.ConfigKey, config.ConfigValue, config.ConfigType, config.IsPublic }),
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);
        return config;
    }

    public async Task DeleteSystemConfigAsync(Guid id, Guid? operatorId = null, CancellationToken ct = default)
    {
        var config = await _db.SystemConfigs.FindAsync([id], ct) ?? throw new BusinessException(404, "系统配置不存在");
        _db.SystemConfigs.Remove(config);
        if (operatorId.HasValue)
        {
            _db.AdminOperationLogs.Add(new AdminOperationLog
            {
                Id = Guid.NewGuid(),
                AdminId = operatorId.Value,
                Operation = "config_delete",
                TargetType = "config",
                TargetId = id,
                Detail = JsonSerializer.Serialize(new { config.ConfigKey }),
                CreatedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureDefaultConfigsAsync(CancellationToken ct)
    {
        if (await _db.SystemConfigs.AnyAsync(ct)) return;
        var now = DateTime.UtcNow;
        _db.SystemConfigs.AddRange(
            NewConfig("site.name", "ByteBite", "string", "系统名称", true, now),
            NewConfig("pickup.code.length", "6", "number", "取货码展示长度", true, now),
            NewConfig("upload.max_mb", "5", "number", "单文件上传大小上限 MB", false, now),
            NewConfig("sms.enabled", "false", "boolean", "短信能力开关，第三方付费服务未接入时保持关闭", false, now)
        );
        await _db.SaveChangesAsync(ct);
    }

    private static SystemConfig NewConfig(string key, string value, string type, string description, bool isPublic, DateTime now)
        => new()
        {
            Id = Guid.NewGuid(),
            ConfigKey = key,
            ConfigValue = value,
            ConfigType = type,
            Description = description,
            IsPublic = isPublic,
            CreatedAt = now,
            UpdatedAt = now
        };

    private static void ValidateConfigValue(string type, string value)
    {
        if (type == "number" && !decimal.TryParse(value, out _)) throw new BusinessException(400, "数字配置值格式不正确");
        if (type == "boolean" && !bool.TryParse(value, out _)) throw new BusinessException(400, "布尔配置值需要为 true 或 false");
        if (type == "json")
        {
            try { JsonDocument.Parse(value); }
            catch { throw new BusinessException(400, "JSON配置值格式不正确"); }
        }
    }
}

public sealed class UpsertSystemConfigInput
{
    public string? ConfigKey { get; set; }
    public string? ConfigValue { get; set; }
    public string? ConfigType { get; set; }
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public Guid? OperatorId { get; set; }
}
