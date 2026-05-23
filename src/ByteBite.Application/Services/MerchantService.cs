using ByteBite.Application.Exceptions;
using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Application.Services;

/// <summary>
/// 商家服务 - 处理商家登录、注册、信息查询、退出登录等业务逻辑
/// </summary>
public class MerchantService
{
    private readonly ByteBiteDbContext _db;

    public MerchantService(ByteBiteDbContext db) { _db = db; }

    /// <summary>
    /// 商家登录 - 验证手机号和密码，校验账号状态，生成Token返回
    /// </summary>
    /// <param name="phone">手机号</param>
    /// <param name="password">明文密码</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>登录成功的商家实体（含Token）</returns>
    /// <exception cref="BusinessException">401-手机号或密码错误, 403-账号被禁用或待审核</exception>
    public async Task<Merchant> LoginAsync(string phone, string password, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Phone == phone, ct);
        if (merchant == null || !PasswordHasher.VerifyPassword(password, merchant.PasswordHash))
            throw new BusinessException(401, "手机号或密码错误");
        // 禁用和待审核状态不允许登录
        if (merchant.Status == "disabled") throw new BusinessException(403, "账号已被禁用");
        if (merchant.Status == "pending") throw new BusinessException(403, "账号待审核");
        merchant.LastLoginAt = DateTime.UtcNow;
        // 生成Base64编码的Token用于前端鉴权
        merchant.Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        await _db.SaveChangesAsync(ct);
        return merchant;
    }

    /// <summary>
    /// 商家注册 - 创建新商家账号，默认待审核状态
    /// </summary>
    /// <param name="phone">手机号（唯一）</param>
    /// <param name="password">明文密码</param>
    /// <param name="nickname">昵称（可选）</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>新注册的商家实体</returns>
    /// <exception cref="BusinessException">400-该手机号已注册</exception>
    public async Task<Merchant> RegisterAsync(string phone, string password, string? nickname, CancellationToken ct = default)
    {
        if (await _db.Merchants.AnyAsync(m => m.Phone == phone, ct))
            throw new BusinessException(400, "该手机号已注册");
        var merchant = new Merchant
        {
            Id = Guid.NewGuid(), Phone = phone, PasswordHash = PasswordHasher.HashPassword(password),
            Nickname = nickname, Status = "pending", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        _db.Merchants.Add(merchant);
        await _db.SaveChangesAsync(ct);
        return merchant;
    }

    /// <summary>
    /// 根据ID获取商家信息
    /// </summary>
    /// <param name="id">商家ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>商家实体</returns>
    /// <exception cref="BusinessException">404-商家不存在</exception>
    public async Task<Merchant> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Merchants.FindAsync([id], ct) ?? throw new BusinessException(404, "商家不存在");

    /// <summary>
    /// 获取商家下的所有店铺列表
    /// </summary>
    /// <param name="merchantId">商家ID</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>店铺列表</returns>
    public async Task<List<Store>> GetStoresAsync(Guid merchantId, CancellationToken ct = default)
        => await _db.Stores.Where(s => s.MerchantId == merchantId).ToListAsync(ct);

    /// <summary>
    /// 商家退出登录 - 清除Token使当前登录凭证失效
    /// </summary>
    /// <param name="merchantId">商家ID</param>
    /// <param name="ct">取消令牌</param>
    public async Task LogoutAsync(Guid merchantId, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.FindAsync([merchantId], ct);
        if (merchant != null) { merchant.Token = null; await _db.SaveChangesAsync(ct); }
    }
}
