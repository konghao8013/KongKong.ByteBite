using ByteBite.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AdminService _adminService;
    private readonly MerchantService _merchantService;
    private readonly CustomerService _customerService;
    private readonly StoreService _storeService;

    public AuthController(AdminService adminService, MerchantService merchantService, CustomerService customerService, StoreService storeService)
    { _adminService = adminService; _merchantService = merchantService; _customerService = customerService; _storeService = storeService; }

    /// <summary>
    /// 统一登录 - 根据账号自动识别角色（管理员/商家/顾客），返回对应的用户信息和角色标识
    /// </summary>
    [HttpPost("login")]
    public async Task<object> UnifiedLogin([FromBody] UnifiedLoginRequest request)
    {
        // 优先尝试管理员登录（用户名匹配）
        if (!request.Account.StartsWith("1") || request.Account.Length != 11)
        {
            try
            {
                var admin = await _adminService.LoginAsync(request.Account, request.Password);
                return new { Role = "admin", Data = admin };
            }
            catch { }
        }

        // 尝试商家登录（手机号匹配）
        try
        {
            var merchant = await _merchantService.LoginAsync(request.Account, request.Password);
            var stores = await _merchantService.GetStoresAsync(merchant.Id);
            return new { Role = "merchant", Data = merchant, StoreId = stores.FirstOrDefault()?.Id };
        }
        catch { }

        // 尝试顾客登录（手机号匹配）
        try
        {
            var customer = await _customerService.LoginAsync(request.Account, request.Password);
            return new { Role = "customer", Data = customer };
        }
        catch { }

        throw new Application.Exceptions.BusinessException(401, "账号或密码错误");
    }
}

public class UnifiedLoginRequest { public string Account { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
