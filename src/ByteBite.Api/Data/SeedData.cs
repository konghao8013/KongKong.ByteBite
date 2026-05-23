using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Api.Data;

/// <summary>
/// 数据库种子数据初始化 - 首次启动时自动创建行业分类、模板、商家、店铺、菜单、订单等基础数据
/// </summary>
public static class SeedData
{
    /// <summary>
    /// 执行种子数据初始化 - 检查关键数据是否存在，不存在则创建
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();

        // 行业分类
        if (!await db.IndustryCategories.AnyAsync())
        {
            await SeedIndustryCategoriesAsync(db);
        }

        // 管理员账号
        var admin = await db.Admins.FirstOrDefaultAsync(a => a.Username == "admin");
        if (admin == null)
        {
            db.Admins.Add(new Admin
            {
                Id = Guid.NewGuid(), Username = "admin", PasswordHash = PasswordHasher.HashPassword("admin123"),
                DisplayName = "系统管理员", Role = "super_admin", Status = "active",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else if (!PasswordHasher.VerifyPassword("admin123", admin.PasswordHash))
        {
            admin.PasswordHash = PasswordHasher.HashPassword("admin123");
            admin.Status = "active";
            await db.SaveChangesAsync();
        }

        // 模板
        if (!await db.StoreTemplates.AnyAsync())
        {
            await SeedTemplateAsync(db);
        }

        // 商家+店铺+菜单
        var merchant = await db.Merchants.FirstOrDefaultAsync(m => m.Phone == "18523978013");
        if (merchant != null)
        {
            // 确保商家状态为active
            if (merchant.Status != "active")
            {
                merchant.Status = "active";
                await db.SaveChangesAsync();
            }

            var store = await db.Stores.FirstOrDefaultAsync(s => s.MerchantId == merchant.Id);
            if (store == null)
            {
                await SeedStoreAndMenuAsync(db, merchant.Id);
                store = await db.Stores.FirstOrDefaultAsync(s => s.MerchantId == merchant.Id);
            }
            else if (!await db.Categories.AnyAsync(c => c.StoreId == store.Id))
            {
                // 店铺存在但没有分类，需要补充菜单数据
                await SeedStoreAndMenuAsync(db, merchant.Id, store.Id);
            }

            // 订单数据
            if (store != null && !await db.Orders.AnyAsync(o => o.StoreId == store.Id))
            {
                await SeedOrdersAsync(db, store.Id);
            }
        }
    }

    /// <summary>
    /// 初始化行业分类（三级：餐饮 → 烧烤 → 重庆特色烧烤）
    /// </summary>
    private static async Task SeedIndustryCategoriesAsync(ByteBiteDbContext db)
    {
        var cat1 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"), Name = "餐饮", Level = 1, SortOrder = 1, Icon = "🍜", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat2 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"), Parent = cat1, Name = "烧烤", Level = 2, SortOrder = 1, Icon = "🔥", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat3 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"), Parent = cat2, Name = "重庆特色烧烤", Level = 3, SortOrder = 1, Icon = "🌶️", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat4 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000004"), Parent = cat2, Name = "东北烧烤", Level = 3, SortOrder = 2, Icon = "🥩", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat5 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000005"), Parent = cat1, Name = "小吃快餐", Level = 2, SortOrder = 2, Icon = "🍔", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat6 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000006"), Parent = cat1, Name = "饮品甜点", Level = 2, SortOrder = 3, Icon = "🧋", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.IndustryCategories.AddRange(cat1, cat2, cat3, cat4, cat5, cat6);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 初始化重庆烧烤模板 - 包含8个分类和20+商品
    /// </summary>
    private static async Task SeedTemplateAsync(ByteBiteDbContext db)
    {
        var industryId = Guid.Parse("a0000000-0000-0000-0000-000000000003");
        var template = new StoreTemplate
        {
            Id = Guid.NewGuid(), Name = "重庆烧烤标准模板", IndustryCategoryId = industryId,
            Description = "重庆风味烧烤店标准菜单模板，包含经典烤串、凉菜、啤酒饮品等",
            SourceType = "manual", Status = "active", UseCount = 0,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        // 模板分类
        var tCats = new[]
        {
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "🔥 热销", CategoryType = "hot", Icon = "🔥", SortOrder = 1, HotTopCount = 10, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "🎁 进店福利", CategoryType = "welfare", Icon = "🎁", SortOrder = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "新疆烧烤", CategoryType = "normal", Icon = "🥩", SortOrder = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "鸡和海鲜", CategoryType = "normal", Icon = "🍗", SortOrder = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "蔬菜类", CategoryType = "normal", Icon = "🥬", SortOrder = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "凉菜", CategoryType = "normal", Icon = "🥗", SortOrder = 6, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "🍺 饮用品类", CategoryType = "normal", Icon = "🍺", SortOrder = 7, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new TemplateCategory { Id = Guid.NewGuid(), Template = template, Name = "🍱 套餐", CategoryType = "combo", Icon = "🍱", SortOrder = 8, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        // 模板商品
        var tProducts = new (int CatIdx, string Name, string Desc, decimal Price, int MinQty)[]
        {
            (1, "凉拌毛豆", "开胃必点", 6, 1), (1, "凉拌花生米", "香酥可口", 6, 1),
            (2, "烤羊肉串", "新疆风味，肥瘦相间", 3, 3), (2, "烤羊排", "整排烤制，外焦里嫩", 38, 1), (2, "烤牛肉串", "嫩牛肉大串", 5, 2), (2, "烤羊腰子", "滋补佳品", 15, 1), (2, "烤五花肉", "肥瘦相间，焦香四溢", 5, 2),
            (3, "烤鸡翅", "蜜汁烤翅，甜香入味", 8, 2), (3, "烤鸡腿", "整腿烤制", 12, 1), (3, "烤生蚝", "蒜蓉烤生蚝，鲜嫩多汁", 15, 1), (3, "烤扇贝", "蒜蓉粉丝扇贝", 15, 1), (3, "烤鱿鱼", "整条烤制，Q弹爽口", 18, 1),
            (4, "烤茄子", "蒜蓉烤茄子，软糯入味", 12, 1), (4, "烤韭菜", "经典素串", 8, 1), (4, "烤金针菇", "锡纸金针菇", 10, 1), (4, "烤玉米", "甜玉米烤制", 8, 1), (4, "烤馒头片", "香脆馒头片", 2, 3),
            (5, "凉拌黄瓜", "爽脆开胃", 10, 1), (5, "口水鸡", "麻辣鲜香", 28, 1),
            (6, "重庆纯生啤酒", "500ml", 8, 1), (6, "雪花啤酒", "500ml", 6, 1), (6, "可乐", "330ml罐装", 4, 1), (6, "矿泉水", "550ml", 2, 1), (6, "王老吉", "310ml罐装", 5, 1),
            (7, "双人烧烤套餐", "含羊肉串10+鸡翅4+烤茄子1+啤酒2", 88, 1), (7, "四人烧烤套餐", "含羊肉串20+鸡翅8+烤生蚝4+烤韭菜2+啤酒4", 168, 1)
        };

        foreach (var (catIdx, name, desc, price, minQty) in tProducts)
        {
            var tp = new TemplateProduct
            {
                Id = Guid.NewGuid(), Template = template, TemplateCategory = tCats[catIdx],
                Name = name, Description = desc, ReferencePrice = price, SortOrder = tCats[catIdx].TemplateProducts.Count + 1,
                MinOrderQty = minQty, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.TemplateProducts.Add(tp);
        }

        db.StoreTemplates.Add(template);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 初始化商家店铺和菜单 - 重庆老灶烧烤完整菜单
    /// </summary>
    private static async Task SeedStoreAndMenuAsync(ByteBiteDbContext db, Guid merchantId, Guid? existingStoreId = null)
    {
        Store store;
        if (existingStoreId != null)
        {
            // 使用已有店铺，补充菜单数据
            store = await db.Stores.FindAsync(existingStoreId) ?? throw new InvalidOperationException("店铺不存在");
            // 更新店铺信息为重庆烧烤风格
            store.Name = "重庆老灶烧烤（观音桥店）";
            store.Description = "正宗重庆风味烧烤，麻辣鲜香，烟火气十足！";
            store.BusinessStatus = "open";
            store.BusinessHoursStart = new TimeOnly(17, 0);
            store.BusinessHoursEnd = new TimeOnly(2, 0);
            store.IndustryCategoryId = Guid.Parse("a0000000-0000-0000-0000-000000000003");
            store.DiningMode = "dine_in,takeaway,delivery";
            store.DeliveryMinAmount = 30;
            store.PackingFee = 1;
            store.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            store = new Store
            {
                Id = Guid.NewGuid(), MerchantId = merchantId, Name = "重庆老灶烧烤（观音桥店）",
                Description = "正宗重庆风味烧烤，麻辣鲜香，烟火气十足！",
                BusinessStatus = "open", BusinessHoursStart = new TimeOnly(17, 0), BusinessHoursEnd = new TimeOnly(2, 0),
                IndustryCategoryId = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                DiningMode = "dine_in,takeaway,delivery", DeliveryMinAmount = 30, PackingFee = 1,
                MonthlySales = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Stores.Add(store);
        }

        // 分类
        var cats = new (string Name, string Type, string Icon, int Sort)[]
        {
            ("🔥 热销", "hot", "🔥", 1), ("🎁 进店福利", "welfare", "🎁", 2),
            ("新疆烧烤", "normal", "🥩", 3), ("鸡和海鲜", "normal", "🍗", 4),
            ("蔬菜类", "normal", "🥬", 5), ("凉菜", "normal", "🥗", 6),
            ("🍺 饮用品类", "normal", "🍺", 7), ("🍱 套餐", "combo", "🍱", 8)
        };

        var catEntities = new List<Category>();
        foreach (var (name, type, icon, sort) in cats)
        {
            var cat = new Category
            {
                Id = Guid.NewGuid(), Store = store, Name = name, CategoryType = type, Icon = icon,
                SortOrder = sort, IsVisible = true, HotTopCount = type == "hot" ? 10 : null,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            catEntities.Add(cat);
            db.Categories.Add(cat);
        }
        await db.SaveChangesAsync();

        // 商品数据
        var products = new (int CatIdx, string Name, string Desc, decimal Price, int MinQty, int MonthlySales, int TotalSales, bool IsCombo)[]
        {
            (1, "凉拌毛豆", "开胃必点", 6, 1, 320, 1580, false),
            (1, "凉拌花生米", "香酥可口", 6, 1, 280, 1320, false),
            (2, "烤羊肉串", "新疆风味，肥瘦相间", 3, 3, 890, 4560, false),
            (2, "烤羊排", "整排烤制，外焦里嫩", 38, 1, 156, 780, false),
            (2, "烤牛肉串", "嫩牛肉大串", 5, 2, 420, 2100, false),
            (2, "烤羊腰子", "滋补佳品", 15, 1, 98, 490, false),
            (2, "烤五花肉", "肥瘦相间，焦香四溢", 5, 2, 560, 2800, false),
            (3, "烤鸡翅", "蜜汁烤翅，甜香入味", 8, 2, 670, 3350, false),
            (3, "烤鸡腿", "整腿烤制", 12, 1, 210, 1050, false),
            (3, "烤生蚝", "蒜蓉烤生蚝，鲜嫩多汁", 15, 1, 380, 1900, false),
            (3, "烤扇贝", "蒜蓉粉丝扇贝", 15, 1, 290, 1450, false),
            (3, "烤鱿鱼", "整条烤制，Q弹爽口", 18, 1, 175, 875, false),
            (4, "烤茄子", "蒜蓉烤茄子，软糯入味", 12, 1, 450, 2250, false),
            (4, "烤韭菜", "经典素串", 8, 1, 520, 2600, false),
            (4, "烤金针菇", "锡纸金针菇", 10, 1, 310, 1550, false),
            (4, "烤玉米", "甜玉米烤制", 8, 1, 240, 1200, false),
            (4, "烤馒头片", "香脆馒头片", 2, 3, 380, 1900, false),
            (5, "凉拌黄瓜", "爽脆开胃", 10, 1, 260, 1300, false),
            (5, "口水鸡", "麻辣鲜香", 28, 1, 85, 425, false),
            (6, "重庆纯生啤酒", "500ml", 8, 1, 720, 3600, false),
            (6, "雪花啤酒", "500ml", 6, 1, 580, 2900, false),
            (6, "可乐", "330ml罐装", 4, 1, 340, 1700, false),
            (6, "矿泉水", "550ml", 2, 1, 460, 2300, false),
            (6, "王老吉", "310ml罐装", 5, 1, 290, 1450, false),
            (7, "双人烧烤套餐", "含羊肉串10+鸡翅4+烤茄子1+啤酒2", 88, 1, 120, 600, true),
            (7, "四人烧烤套餐", "含羊肉串20+鸡翅8+烤生蚝4+烤韭菜2+啤酒4", 168, 1, 65, 325, true)
        };

        var productEntities = new List<Product>();
        foreach (var (catIdx, name, desc, price, minQty, monthlySales, totalSales, isCombo) in products)
        {
            var p = new Product
            {
                Id = Guid.NewGuid(), Store = store, Category = catEntities[catIdx],
                Name = name, Description = desc, BasePrice = price, Status = "on",
                SortOrder = productEntities.Count + 1, MinOrderQty = minQty,
                MonthlySales = monthlySales, TotalSales = totalSales, IsCombo = isCombo,
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            productEntities.Add(p);
            db.Products.Add(p);
        }
        await db.SaveChangesAsync();

        // 商品规格（羊肉串份量、五花肉份量、鸡翅口味、生蚝份量、啤酒规格）
        var specGroups = new (int ProductIdx, string Name, (string OptionName, decimal ExtraPrice, bool IsDefault)[] Options)[]
        {
            (2, "份量", new[] { ("小串(1串)", 0m, true), ("大串(2串)", 3m, false) }),
            (6, "份量", new[] { ("小份", 0m, true), ("大份", 5m, false) }),
            (7, "口味", new[] { ("蜜汁", 0m, true), ("香辣", 0m, false), ("蒜香", 0m, false) }),
            (9, "份量", new[] { ("半打(6个)", 0m, true), ("一打(12个)", 75m, false) }),
            (19, "规格", new[] { ("瓶装500ml", 0m, true), ("罐装330ml", -2m, false) })
        };

        foreach (var (productIdx, groupName, options) in specGroups)
        {
            var sg = new SpecGroup
            {
                Id = Guid.NewGuid(), Product = productEntities[productIdx], Name = groupName,
                SortOrder = 1, IsRequired = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.SpecGroups.Add(sg);

            foreach (var (optName, extraPrice, isDefault) in options)
            {
                db.SpecOptions.Add(new SpecOption
                {
                    Id = Guid.NewGuid(), SpecGroup = sg, Name = optName, ExtraPrice = extraPrice,
                    SortOrder = 1, IsDefault = isDefault, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // 优惠规则
        db.DiscountRules.AddRange(
            new DiscountRule
            {
                Id = Guid.NewGuid(), Store = store, Name = "满100减10", DiscountType = "full_reduction",
                ThresholdAmount = 100, DiscountAmount = 10, ApplyScope = "all", AllowStack = false,
                StartTime = DateTime.UtcNow.AddDays(-30), EndTime = DateTime.UtcNow.AddDays(365),
                Status = "active", UsedCount = 45, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new DiscountRule
            {
                Id = Guid.NewGuid(), Store = store, Name = "满200减30", DiscountType = "full_reduction",
                ThresholdAmount = 200, DiscountAmount = 30, ApplyScope = "all", AllowStack = false,
                StartTime = DateTime.UtcNow.AddDays(-30), EndTime = DateTime.UtcNow.AddDays(365),
                Status = "active", UsedCount = 22, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new DiscountRule
            {
                Id = Guid.NewGuid(), Store = store, Name = "烧烤类8折", DiscountType = "discount",
                DiscountRate = 80, ApplyScope = "category", AllowStack = false,
                StartTime = DateTime.UtcNow.AddDays(-7), EndTime = DateTime.UtcNow.AddDays(30),
                Status = "active", UsedCount = 18, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 初始化订单数据 - 包含近7天各种状态的订单，用于报表验收
    /// </summary>
    private static async Task SeedOrdersAsync(ByteBiteDbContext db, Guid storeId)
    {
        var now = DateTime.UtcNow;
        var orders = new List<Order>();

        // 7天前已完成订单
        CreateCompletedOrder(db, storeId, "20260514001", "A1B2", "dine_in", 86, 0, 86, 0, now.AddDays(-7).AddHours(18), ref orders);
        CreateCompletedOrder(db, storeId, "20260514002", "C3D4", "takeaway", 42, 0, 43, 1, now.AddDays(-7).AddHours(19), ref orders);
        CreateCompletedOrder(db, storeId, "20260514003", "E5F6", "dine_in", 128, 10, 118, 0, now.AddDays(-7).AddHours(20), ref orders);

        // 5天前已完成
        CreateCompletedOrder(db, storeId, "20260516001", "G7H8", "dine_in", 56, 0, 56, 0, now.AddDays(-5).AddHours(18.5), ref orders);
        CreateCompletedOrder(db, storeId, "20260516002", "I9J0", "delivery", 88, 0, 89, 1, now.AddDays(-5).AddHours(19.25), ref orders);

        // 3天前已完成
        CreateCompletedOrder(db, storeId, "20260518001", "K1L2", "dine_in", 168, 30, 138, 0, now.AddDays(-3).AddHours(19), ref orders);
        CreateCompletedOrder(db, storeId, "20260518002", "M3N4", "takeaway", 35, 0, 36, 1, now.AddDays(-3).AddHours(20), ref orders);
        CreateCompletedOrder(db, storeId, "20260518003", "O5P6", "dine_in", 72, 0, 72, 0, now.AddDays(-3).AddHours(21), ref orders);

        // 昨天已完成
        CreateCompletedOrder(db, storeId, "20260520001", "Q7R8", "dine_in", 96, 0, 96, 0, now.AddDays(-1).AddHours(18), ref orders);
        CreateCompletedOrder(db, storeId, "20260520002", "S9T0", "dine_in", 145, 10, 135, 0, now.AddDays(-1).AddHours(19.5), ref orders);
        CreateCompletedOrder(db, storeId, "20260520003", "U1V2", "takeaway", 52, 0, 53, 1, now.AddDays(-1).AddHours(20), ref orders);
        CreateCompletedOrder(db, storeId, "20260520004", "W3X4", "dine_in", 210, 30, 180, 0, now.AddDays(-1).AddHours(21), ref orders);

        // 今天 - 各种状态
        var todayOrders = new (string No, string Code, string Mode, decimal Total, decimal Disc, decimal Actual, decimal Pack, string Status, int MinAgo)[]
        {
            ("20260521001", "Y5Z6", "dine_in", 78, 0, 78, 0, "ready", 25),
            ("20260521002", "A7B8", "dine_in", 115, 10, 105, 0, "preparing", 15),
            ("20260521003", "C9D0", "takeaway", 45, 0, 46, 1, "accepted", 5),
            ("20260521004", "E1F2", "dine_in", 168, 30, 138, 0, "pending", 2),
            ("20260521005", "G3H4", "delivery", 62, 0, 63, 1, "pending", 1)
        };

        foreach (var (no, code, mode, total, disc, actual, pack, status, minAgo) in todayOrders)
        {
            var created = now.AddMinutes(-minAgo);
            var order = new Order
            {
                Id = Guid.NewGuid(), StoreId = storeId, DeviceId = $"DEV-{code}",
                OrderNo = no, PickupCode = code, DiningMode = mode,
                TotalAmount = total, DiscountAmount = disc, ActualAmount = actual, PackingFee = pack,
                Status = status, CreatedAt = created, UpdatedAt = now
            };
            // 根据状态设置时间戳
            if (status != "pending") order.AcceptedAt = created.AddMinutes(2);
            if (status is "preparing" or "ready") order.PreparingAt = created.AddMinutes(5);
            if (status == "ready") order.ReadyAt = created.AddMinutes(15);
            orders.Add(order);
            db.Orders.Add(order);
        }

        // 已取消订单
        db.Orders.Add(new Order
        {
            Id = Guid.NewGuid(), StoreId = storeId, DeviceId = "DEV-I5J6",
            OrderNo = "20260519001", PickupCode = "I5J6", DiningMode = "dine_in",
            TotalAmount = 38, DiscountAmount = 0, ActualAmount = 38, PackingFee = 0,
            Status = "cancelled", RejectReason = "商家忙碌，暂不接单",
            CreatedAt = now.AddDays(-2).AddHours(22), CancelledAt = now.AddDays(-2).AddHours(22).AddMinutes(3),
            UpdatedAt = now.AddDays(-2).AddHours(22).AddMinutes(3)
        });

        await db.SaveChangesAsync();

        // 订单项
        var allProducts = await db.Products.Where(p => p.StoreId == storeId).ToListAsync();

        // 为每个已完成订单创建订单项
        foreach (var order in orders.Where(o => o.Status == "completed").Take(8))
        {
            // 每个订单随机选2-4个商品
            var randomProducts = allProducts.OrderBy(p => Guid.NewGuid()).Take(new Random().Next(2, 5)).ToList();
            foreach (var product in randomProducts)
            {
                var qty = product.MinOrderQty > 1 ? product.MinOrderQty : new Random().Next(1, 5);
                db.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(), OrderId = order.Id, ProductId = product.Id,
                    ProductName = product.Name, Quantity = qty, UnitPrice = product.BasePrice, TotalPrice = qty * product.BasePrice,
                    IsCombo = product.IsCombo, CreatedAt = order.CreatedAt
                });
            }
        }

        // 为今天的活跃订单创建订单项
        foreach (var order in orders.Where(o => o.Status != "completed" && o.Status != "cancelled"))
        {
            var randomProducts = allProducts.OrderBy(p => Guid.NewGuid()).Take(new Random().Next(2, 4)).ToList();
            foreach (var product in randomProducts)
            {
                var qty = product.MinOrderQty > 1 ? product.MinOrderQty : new Random().Next(1, 4);
                db.OrderItems.Add(new OrderItem
                {
                    Id = Guid.NewGuid(), OrderId = order.Id, ProductId = product.Id,
                    ProductName = product.Name, Quantity = qty, UnitPrice = product.BasePrice, TotalPrice = qty * product.BasePrice,
                    IsCombo = product.IsCombo, CreatedAt = order.CreatedAt
                });
            }
        }

        await db.SaveChangesAsync();
    }

    /// <summary>
    /// 创建已完成订单 - 模拟完整的订单生命周期时间戳
    /// </summary>
    private static void CreateCompletedOrder(ByteBiteDbContext db, Guid storeId, string orderNo, string pickupCode, string diningMode, decimal total, decimal discount, decimal actual, decimal packingFee, DateTime createdAt, ref List<Order> orders)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(), StoreId = storeId, DeviceId = $"DEV-{pickupCode}",
            OrderNo = orderNo, PickupCode = pickupCode, DiningMode = diningMode,
            TotalAmount = total, DiscountAmount = discount, ActualAmount = actual, PackingFee = packingFee,
            Status = "completed", CreatedAt = createdAt,
            AcceptedAt = createdAt.AddMinutes(2), PreparingAt = createdAt.AddMinutes(5),
            ReadyAt = createdAt.AddMinutes(15), CompletedAt = createdAt.AddMinutes(20),
            UpdatedAt = createdAt.AddMinutes(20)
        };
        orders.Add(order);
        db.Orders.Add(order);
    }
}
