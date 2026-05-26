using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 顾客服务 - 处理顾客注册、登录、数据合并、退出登录等业务逻辑
/// </summary>
public class CustomerService
{
    private readonly ByteBiteDbContext _db;

    public CustomerService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 顾客注册 - 创建新顾客账号，支持关联设备ID
    /// </summary>
    /// <param name="phone">手机号（唯一）</param>
    /// <param name="password">明文密码</param>
    /// <param name="nickname">昵称（可选）</param>
    /// <param name="deviceId">设备ID（可选，用于数据合并）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新注册的顾客实体</returns>
    /// <exception cref="BusinessException">400-该手机号已注册</exception>
    public async Task<Customer> RegisterAsync(string phone, string password, string? nickname, string? deviceId, CancellationToken ct = default)
    {
        if (await _db.Customers.AnyAsync(c => c.Phone == phone, ct)) throw new BusinessException(400, "该手机号已注册");
        var entity = new Customer
        {
            Id = Guid.NewGuid(), Phone = phone, PasswordHash = PasswordHasher.HashPassword(password),
            Nickname = nickname, DeviceId = deviceId, IsRegistered = true,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Customers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity;
    }

    /// <summary>
    /// 顾客登录 - 验证手机号和密码，生成Token返回
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <param name="password">明文密码</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>登录成功的顾客实体（含Token）</returns>
    /// <exception cref="BusinessException">404-用户不存在, 401-密码错误</exception>
    public async Task<Customer> LoginAsync(string phone, string password, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Phone == phone, ct) ?? throw new BusinessException(404, "用户不存在");
        if (customer.PasswordHash != null && !PasswordHasher.VerifyPassword(password, customer.PasswordHash))
            throw new BusinessException(401, "密码错误");
        customer.LastLoginAt = DateTime.UtcNow;
        // 生成Base64编码的Token用于前端鉴权
        customer.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    /// <summary>
    /// 根据ID获取顾客信息
    /// </summary>
    /// <param name="id">顾客ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>顾客实体</returns>
    /// <exception cref="BusinessException">404-用户不存在</exception>
    public async Task<Customer> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Customers.FindAsync([id], ct) ?? throw new BusinessException(404, "用户不存在");

    public async Task<Customer> EnsureAnonymousAsync(string deviceId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(deviceId)) throw new BusinessException(400, "设备标识不能为空");
        var normalizedDeviceId = deviceId.Trim();
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.DeviceId == normalizedDeviceId && !c.IsRegistered, ct);
        if (customer != null) return customer;

        customer = new Customer
        {
            Id = Guid.NewGuid(),
            DeviceId = normalizedDeviceId,
            IsRegistered = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);
        return customer;
    }

    /// <summary>
    /// 合并匿名数据 - 将设备ID关联的订单合并到注册顾客账号
    /// </summary>
    /// <param name="targetCustomerId">目标顾客ID</param>
    /// <param name="sourceDeviceId">源设备ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>合并的订单数量</returns>
    public async Task<int> MergeDataAsync(Guid targetCustomerId, string sourceDeviceId, CancellationToken ct = default)
    {
        var orders = await _db.Orders.Where(o => o.DeviceId == sourceDeviceId).ToListAsync(ct);
        // 将匿名订单关联到注册顾客，清除设备ID标记
        foreach (var order in orders) { order.CustomerId = targetCustomerId; order.DeviceId = null; }
        await _db.SaveChangesAsync(ct);
        return orders.Count;
    }

    /// <summary>
    /// 顾客退出登录 - 清除Token使当前登录凭证失效
    /// </summary>
    /// <param name="customerId">顾客ID</param>
    /// <param name="ct">取消令牌</param>
    public async Task LogoutAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _db.Customers.FindAsync([customerId], ct);
        if (customer != null) { customer.Token = null; await _db.SaveChangesAsync(ct); }
    }
}
