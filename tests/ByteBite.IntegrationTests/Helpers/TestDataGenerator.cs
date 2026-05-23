using System.Net.Http.Json;
using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.DTOs.Store;
using ByteBite.Application.DTOs.Category;
using ByteBite.Application.DTOs.Product;
using ByteBite.Application.DTOs.Customer;
using ByteBite.Application.DTOs.Order;
using ByteBite.Application.DTOs.Discount;
using ByteBite.Application.DTOs.Template;
using ByteBite.Application.DTOs.Admin;
using ByteBite.Application.DTOs.Dashboard;
using ByteBite.Application.DTOs.Common;

namespace ByteBite.IntegrationTests.Helpers;

/// <summary>
/// 测试数据生成器 - 创建各模块的测试请求对象
/// </summary>
public static class TestDataGenerator
{
    /// <summary>生成唯一手机号（基于时间戳避免冲突）</summary>
    public static string GeneratePhone() =>
        $"139{DateTime.UtcNow.Ticks % 100000000:D8}";

    /// <summary>生成商家注册请求</summary>
    public static RegisterMerchantRequest CreateMerchantRegisterRequest(string? phone = null)
    {
        return new RegisterMerchantRequest
        {
            Phone = phone ?? GeneratePhone(),
            Password = "Test123456!",
            StoreName = "测试烧烤店"
        };
    }

    /// <summary>生成商家登录请求</summary>
    public static LoginMerchantRequest CreateMerchantLoginRequest(string phone, string password = "Test123456!")
    {
        return new LoginMerchantRequest
        {
            Phone = phone,
            Password = password
        };
    }

    /// <summary>生成创建店铺请求</summary>
    public static CreateStoreRequest CreateStoreRequest(Guid merchantId)
    {
        return new CreateStoreRequest
        {
            Name = "测试烧烤店",
            Description = "一家测试用的烧烤店",
            IndustryCategoryId = null
        };
    }

    /// <summary>生成创建分类请求</summary>
    public static CreateCategoryRequest CreateCategoryRequest(string name = "烧烤类", string categoryType = "normal")
    {
        return new CreateCategoryRequest
        {
            Name = name,
            CategoryType = categoryType,
            SortOrder = 1
        };
    }

    /// <summary>生成创建商品请求</summary>
    public static CreateProductRequest CreateProductRequest(Guid categoryId, string name = "烤羊肉串")
    {
        return new CreateProductRequest
        {
            Name = name,
            BasePrice = 15,
            Description = "新鲜羊肉炭火烤制",
            ImageUrl = "",
            CategoryId = categoryId,
            MinOrderQty = 1,
            IsCombo = false
        };
    }

    /// <summary>生成顾客注册请求</summary>
    public static RegisterCustomerRequest CreateCustomerRegisterRequest(string? phone = null)
    {
        return new RegisterCustomerRequest
        {
            Phone = phone ?? GeneratePhone(),
            VerifyCode = "1234",
            Nickname = "测试顾客",
            DeviceId = $"device_test_{Guid.NewGuid():N}"
        };
    }

    /// <summary>生成创建订单请求</summary>
    public static CreateOrderRequest CreateOrderRequest(Guid storeId, List<OrderItemRequest> items)
    {
        return new CreateOrderRequest
        {
            StoreId = storeId,
            DiningMode = "dine_in",
            Items = items,
            Remark = "测试订单"
        };
    }

    /// <summary>生成创建订单项请求</summary>
    public static OrderItemRequest CreateOrderItemRequest(Guid productId, int quantity = 1)
    {
        return new OrderItemRequest
        {
            ProductId = productId,
            Quantity = quantity,
            SelectedSpecOptionIds = new List<Guid>(),
            Remark = ""
        };
    }

    /// <summary>生成创建优惠规则请求</summary>
    public static CreateDiscountRuleRequest CreateDiscountRuleRequest(Guid storeId)
    {
        return new CreateDiscountRuleRequest
        {
            Name = "满50减5",
            DiscountType = "full_reduction",
            ThresholdAmount = 50,
            DiscountAmount = 5,
            DiscountRate = 0,
            ApplyScope = "all",
            AllowStack = false,
            StartTime = DateTime.Today,
            EndTime = DateTime.Today.AddDays(30)
        };
    }

    /// <summary>生成管理员登录请求</summary>
    public static LoginAdminRequest CreateAdminLoginRequest(string username = "super_admin", string password = "admin123")
    {
        return new LoginAdminRequest
        {
            Username = username,
            Password = password
        };
    }
}