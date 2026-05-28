using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
    public async Task<Customer> RegisterAsync(string? phone, string? username, string password, string? nickname, string? deviceId, CancellationToken ct = default)
    {
        var normalizedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        var normalizedUsername = string.IsNullOrWhiteSpace(username) ? null : username.Trim();

        if (normalizedPhone == null && normalizedUsername == null)
            throw new BusinessException(400, "请填写手机号或账号名");
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new BusinessException(400, "密码至少需要6位");
        if (normalizedPhone != null && await _db.Customers.AnyAsync(c => c.Phone == normalizedPhone && c.IsRegistered, ct))
            throw new BusinessException(400, "该手机号已注册");
        if (normalizedUsername != null && await _db.Customers.AnyAsync(c => c.Username == normalizedUsername && c.IsRegistered, ct))
            throw new BusinessException(400, "该账号名已注册");

        var now = DateTime.UtcNow;
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            Phone = normalizedPhone,
            Username = normalizedUsername,
            PasswordHash = PasswordHasher.HashPassword(password),
            Nickname = nickname ?? (normalizedPhone != null && normalizedPhone.Length >= 4 ? normalizedPhone[^4..] : normalizedUsername),
            DeviceId = deviceId,
            IsRegistered = true,
            CreatedAt = now,
            UpdatedAt = now,
            LastLoginAt = now,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
        };
        _db.Customers.Add(entity);
        await _db.SaveChangesAsync(ct);
        if (!string.IsNullOrWhiteSpace(deviceId))
            await MergeDataAsync(entity.Id, deviceId, ct);
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
    public async Task<Customer> LoginAsync(string account, string password, string? deviceId = null, CancellationToken ct = default)
    {
        var normalizedAccount = account.Trim();
        var customer = await _db.Customers.FirstOrDefaultAsync(c =>
            c.IsRegistered && (c.Phone == normalizedAccount || c.Username == normalizedAccount), ct)
            ?? throw new BusinessException(404, "用户不存在");
        if (string.IsNullOrWhiteSpace(customer.PasswordHash) || !PasswordHasher.VerifyPassword(password, customer.PasswordHash))
            throw new BusinessException(401, "密码错误");
        customer.LastLoginAt = DateTime.UtcNow;
        // 生成Base64编码的Token用于前端鉴权
        customer.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        await _db.SaveChangesAsync(ct);
        if (!string.IsNullOrWhiteSpace(deviceId))
            await MergeDataAsync(customer.Id, deviceId, ct);
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

    public async Task<DataMergeResultDto> GetMergePreviewAsync(string sourceDeviceId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sourceDeviceId))
            return new DataMergeResultDto { NeedMerge = false };

        var summaries = await BuildStoreMergeSummariesAsync(sourceDeviceId.Trim(), ct);
        return new DataMergeResultDto
        {
            NeedMerge = summaries.Count > 0,
            StoreSummaries = summaries,
            OrdersMerged = summaries.Sum(s => s.ActiveOrders + s.CompletedOrders),
            CartItemsMerged = summaries.Sum(s => s.CartItems),
            PickupCodesMerged = summaries.Sum(s => s.ActiveOrders)
        };
    }

    /// <summary>
    /// 合并匿名数据 - 将设备ID关联的订单、最近店铺、会话和服务端购物车合并到注册顾客账号。
    /// </summary>
    public async Task<DataMergeResultDto> MergeDataAsync(Guid targetCustomerId, string sourceDeviceId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sourceDeviceId)) return new DataMergeResultDto { NeedMerge = false };
        var normalizedDeviceId = sourceDeviceId.Trim();
        var summaries = await BuildStoreMergeSummariesAsync(normalizedDeviceId, ct);

        var orders = await _db.Orders.Where(o => o.DeviceId == normalizedDeviceId).ToListAsync(ct);
        foreach (var order in orders) { order.CustomerId = targetCustomerId; order.DeviceId = null; }

        var visits = await _db.CustomerStoreVisits.Where(v => v.DeviceId == normalizedDeviceId).ToListAsync(ct);
        foreach (var visit in visits)
        {
            var existing = await _db.CustomerStoreVisits.FirstOrDefaultAsync(v => v.CustomerId == targetCustomerId && v.StoreId == visit.StoreId, ct);
            if (existing == null)
            {
                visit.CustomerId = targetCustomerId;
                visit.DeviceId = null;
            }
            else
            {
                existing.LastVisitedAt = existing.LastVisitedAt > visit.LastVisitedAt ? existing.LastVisitedAt : visit.LastVisitedAt;
                existing.LastOrderedAt = MaxDate(existing.LastOrderedAt, visit.LastOrderedAt);
                _db.CustomerStoreVisits.Remove(visit);
            }
        }

        var conversations = await _db.Conversations.Where(c => c.DeviceId == normalizedDeviceId).ToListAsync(ct);
        foreach (var conversation in conversations)
        {
            conversation.CustomerId = targetCustomerId;
            conversation.DeviceId = null;
        }

        var cartItemsMerged = await MergeCartSessionsAsync(targetCustomerId, normalizedDeviceId, ct);
        var sourceCustomer = await _db.Customers.FirstOrDefaultAsync(c => c.DeviceId == normalizedDeviceId && !c.IsRegistered, ct);
        _db.DataMergeLogs.Add(new DataMergeLog
        {
            Id = Guid.NewGuid(),
            TargetCustomerId = targetCustomerId,
            SourceDeviceId = normalizedDeviceId,
            SourceCustomerId = sourceCustomer?.Id,
            MergeType = "device_to_account",
            OrdersMerged = orders.Count,
            CartItemsMerged = cartItemsMerged,
            PickupCodesMerged = orders.Count(o => o.Status is "pending" or "accepted" or "preparing" or "ready"),
            ConflictsResolved = 0,
            MergeDetail = JsonSerializer.Serialize(new { summaries }),
            Status = "completed",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync(ct);
        return new DataMergeResultDto
        {
            NeedMerge = summaries.Count > 0,
            StoreSummaries = summaries,
            OrdersMerged = orders.Count,
            CartItemsMerged = cartItemsMerged,
            PickupCodesMerged = orders.Count(o => o.Status is "pending" or "accepted" or "preparing" or "ready")
        };
    }

    public async Task<List<CustomerCartStoreDto>> GetCartAsync(Guid? customerId, string? deviceId, Guid? storeId = null, CancellationToken ct = default)
    {
        var session = await FindCartSessionAsync(customerId, deviceId, ct);
        var stores = ParseCartData(session?.CartData);
        if (storeId.HasValue) stores = stores.Where(s => s.StoreId == storeId.Value).ToList();
        return stores;
    }

    public async Task<CustomerCartStoreDto> SaveCartAsync(SaveCustomerCartInput input, CancellationToken ct = default)
    {
        if (!input.StoreId.HasValue) throw new BusinessException(400, "店铺ID不能为空");
        var session = await GetOrCreateCartSessionAsync(input.CustomerId, input.DeviceId, ct);
        var stores = ParseCartData(session.CartData);
        var now = DateTime.UtcNow;
        var storeCart = new CustomerCartStoreDto
        {
            StoreId = input.StoreId.Value,
            Items = NormalizeCartItems(input.Items),
            UpdatedAt = now
        };

        var index = stores.FindIndex(s => s.StoreId == input.StoreId.Value);
        if (index >= 0) stores[index] = storeCart;
        else stores.Add(storeCart);

        session.CartData = JsonSerializer.Serialize(stores);
        session.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);
        return storeCart;
    }

    public async Task<List<CustomerCartStoreDto>> MergeLocalCartAsync(MergeCustomerCartInput input, CancellationToken ct = default)
    {
        var session = await GetOrCreateCartSessionAsync(input.CustomerId, input.DeviceId, ct);
        var remoteStores = ParseCartData(session.CartData);
        var merged = MergeCartStores(remoteStores, input.Stores ?? [], out _, DateTime.UtcNow);
        session.CartData = JsonSerializer.Serialize(merged);
        session.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return merged;
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

    private static DateTime? MaxDate(DateTime? left, DateTime? right)
    {
        if (left == null) return right;
        if (right == null) return left;
        return left > right ? left : right;
    }

    private async Task<CustomerSession?> FindCartSessionAsync(Guid? customerId, string? deviceId, CancellationToken ct)
    {
        if (customerId.HasValue)
            return await _db.CustomerSessions.FirstOrDefaultAsync(s => s.CustomerId == customerId.Value && s.DeviceId == null, ct);
        if (!string.IsNullOrWhiteSpace(deviceId))
            return await _db.CustomerSessions.FirstOrDefaultAsync(s => s.DeviceId == deviceId.Trim(), ct);
        return null;
    }

    private async Task<CustomerSession> GetOrCreateCartSessionAsync(Guid? customerId, string? deviceId, CancellationToken ct)
    {
        var session = await FindCartSessionAsync(customerId, deviceId, ct);
        if (session != null) return session;

        var resolvedCustomerId = customerId;
        if (!resolvedCustomerId.HasValue)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) throw new BusinessException(400, "缺少顾客身份信息");
            resolvedCustomerId = (await EnsureAnonymousAsync(deviceId, ct)).Id;
        }

        var now = DateTime.UtcNow;
        session = new CustomerSession
        {
            Id = Guid.NewGuid(),
            CustomerId = resolvedCustomerId.Value,
            DeviceId = customerId.HasValue ? null : deviceId?.Trim(),
            CartData = "[]",
            CreatedAt = now,
            UpdatedAt = now
        };
        _db.CustomerSessions.Add(session);
        return session;
    }

    private async Task<int> MergeCartSessionsAsync(Guid targetCustomerId, string sourceDeviceId, CancellationToken ct)
    {
        var sourceSession = await FindCartSessionAsync(null, sourceDeviceId, ct);
        if (sourceSession == null || string.IsNullOrWhiteSpace(sourceSession.CartData)) return 0;

        var targetSession = await GetOrCreateCartSessionAsync(targetCustomerId, null, ct);
        var merged = MergeCartStores(ParseCartData(targetSession.CartData), ParseCartData(sourceSession.CartData), out var mergedCount, DateTime.UtcNow);
        targetSession.CartData = JsonSerializer.Serialize(merged);
        targetSession.UpdatedAt = DateTime.UtcNow;
        sourceSession.CartData = "[]";
        sourceSession.UpdatedAt = DateTime.UtcNow;
        return mergedCount;
    }

    private async Task<List<StoreMergeSummaryDto>> BuildStoreMergeSummariesAsync(string sourceDeviceId, CancellationToken ct)
    {
        var orderGroups = await _db.Orders
            .Include(o => o.Store)
            .Where(o => o.DeviceId == sourceDeviceId)
            .GroupBy(o => new { o.StoreId, o.Store.Name })
            .Select(g => new StoreMergeSummaryDto
            {
                StoreId = g.Key.StoreId,
                StoreName = g.Key.Name,
                ActiveOrders = g.Count(o => o.Status == "pending" || o.Status == "accepted" || o.Status == "preparing" || o.Status == "ready"),
                CompletedOrders = g.Count(o => o.Status == "completed" || o.Status == "cancelled" || o.Status == "rejected")
            })
            .ToListAsync(ct);

        var sourceSession = await FindCartSessionAsync(null, sourceDeviceId, ct);
        foreach (var cart in ParseCartData(sourceSession?.CartData))
        {
            var summary = orderGroups.FirstOrDefault(s => s.StoreId == cart.StoreId);
            if (summary == null)
            {
                var storeName = await _db.Stores.Where(s => s.Id == cart.StoreId).Select(s => s.Name).FirstOrDefaultAsync(ct) ?? "店铺";
                summary = new StoreMergeSummaryDto { StoreId = cart.StoreId, StoreName = storeName };
                orderGroups.Add(summary);
            }
            summary.CartItems = cart.Items.Sum(i => i.Quantity);
        }

        return orderGroups;
    }

    private static List<CustomerCartStoreDto> ParseCartData(string? cartData)
    {
        if (string.IsNullOrWhiteSpace(cartData)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<CustomerCartStoreDto>>(cartData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static List<CustomerCartStoreDto> MergeCartStores(List<CustomerCartStoreDto> target, IEnumerable<CustomerCartStoreDto> source, out int mergedCount, DateTime now)
    {
        mergedCount = 0;
        foreach (var sourceStore in source)
        {
            var targetStore = target.FirstOrDefault(s => s.StoreId == sourceStore.StoreId);
            if (targetStore == null)
            {
                var clone = new CustomerCartStoreDto { StoreId = sourceStore.StoreId, Items = NormalizeCartItems(sourceStore.Items), UpdatedAt = now };
                target.Add(clone);
                mergedCount += clone.Items.Sum(i => i.Quantity);
                continue;
            }

            foreach (var item in NormalizeCartItems(sourceStore.Items))
            {
                var existing = targetStore.Items.FirstOrDefault(i => CartItemKey(i) == CartItemKey(item));
                if (existing == null) targetStore.Items.Add(item);
                else existing.Quantity += item.Quantity;
                mergedCount += item.Quantity;
            }
            targetStore.UpdatedAt = now;
        }
        return target;
    }

    private static List<CustomerCartItemDto> NormalizeCartItems(IEnumerable<CustomerCartItemDto>? items)
        => (items ?? [])
            .Where(i => i.ProductId != Guid.Empty && i.Quantity > 0)
            .Select(i => new CustomerCartItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ImageUrl = i.ImageUrl,
                Price = i.Price,
                Quantity = i.Quantity,
                Specs = i.Specs ?? [],
                Remark = i.Remark
            })
            .ToList();

    private static string CartItemKey(CustomerCartItemDto item)
    {
        var specs = string.Join("|", (item.Specs ?? []).Select(s => s.OptionId).OrderBy(id => id));
        return $"{item.ProductId:N}:{specs}:{item.Remark}";
    }
}

public sealed class DataMergeResultDto
{
    public bool NeedMerge { get; set; }
    public List<StoreMergeSummaryDto> StoreSummaries { get; set; } = [];
    public int OrdersMerged { get; set; }
    public int CartItemsMerged { get; set; }
    public int PickupCodesMerged { get; set; }
}

public sealed class StoreMergeSummaryDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int ActiveOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CartItems { get; set; }
}

public sealed class SaveCustomerCartInput
{
    public Guid? CustomerId { get; set; }
    public string? DeviceId { get; set; }
    public Guid? StoreId { get; set; }
    public List<CustomerCartItemDto> Items { get; set; } = [];
}

public sealed class MergeCustomerCartInput
{
    public Guid? CustomerId { get; set; }
    public string? DeviceId { get; set; }
    public List<CustomerCartStoreDto>? Stores { get; set; }
}

public sealed class CustomerCartStoreDto
{
    public Guid StoreId { get; set; }
    public List<CustomerCartItemDto> Items { get; set; } = [];
    public DateTime UpdatedAt { get; set; }
}

public sealed class CustomerCartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public List<CustomerCartSpecDto> Specs { get; set; } = [];
    public string? Remark { get; set; }
}

public sealed class CustomerCartSpecDto
{
    public Guid SpecGroupId { get; set; }
    public string SpecGroupName { get; set; } = string.Empty;
    public Guid OptionId { get; set; }
    public string OptionName { get; set; } = string.Empty;
    public decimal ExtraPrice { get; set; }
}
