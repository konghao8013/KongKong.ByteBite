using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Api.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var seeders = new IDataSeeder[]
        {
            new AdminSeeder(),
            new IndustryCategorySeeder(),
            new TemplateSeeder(),
            new StoreMenuSeeder(),
            new OrderSeeder(),
            new StoreCodeSeeder(),
        };

        foreach (var seeder in seeders.OrderBy(s => s.Order))
        {
            await seeder.SeedAsync(services);
        }
    }
}

file class TemplateSeeder : IDataSeeder
{
    public int Order => 30;

    public async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();

        if (await db.StoreTemplates.AnyAsync()) return;

        var industryId = Guid.Parse("a0000000-0000-0000-0000-000000000003");
        var template = new StoreTemplate
        {
            Id = Guid.NewGuid(), Name = "重庆烧烤标准模板", IndustryCategoryId = industryId,
            Description = "重庆风味烧烤店标准菜单模板，包含经典烤串、凉菜、啤酒饮品等",
            SourceType = "manual", Status = "active", UseCount = 0,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

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
}

file class StoreMenuSeeder : IDataSeeder
{
    private const string DemoMerchantName = "演示商家";
    private const string DemoStoreName = "重庆老灶烧烤（观音桥店）";
    private const string DemoStoreDescription = "正宗重庆风味烧烤，麻辣鲜香，烟火气十足！";

    public int Order => 40;

    public async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();

        var merchant = await db.Merchants.FirstOrDefaultAsync(m => m.Phone == "18523978013");
        if (merchant == null)
        {
            merchant = new Merchant
            {
                Id = Guid.NewGuid(),
                Phone = "18523978013",
                PasswordHash = PasswordHasher.HashPassword("123456"),
                Nickname = DemoMerchantName,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            db.Merchants.Add(merchant);
            await db.SaveChangesAsync();
        }
        else if (merchant.Status != "active")
        {
            merchant.Status = "active";
            await db.SaveChangesAsync();
        }

        var store = await db.Stores.FirstOrDefaultAsync(s => s.MerchantId == merchant.Id);
        if (store == null)
        {
            store = new Store
            {
                Id = Guid.NewGuid(), MerchantId = merchant.Id, Name = DemoStoreName,
                StoreCode = Base36Encoder.Encode(1),
                Description = DemoStoreDescription,
                BusinessStatus = "open", BusinessHoursStart = new TimeOnly(17, 0), BusinessHoursEnd = new TimeOnly(2, 0),
                IndustryCategoryId = Guid.Parse("a0000000-0000-0000-0000-000000000003"),
                DiningMode = "dine_in,takeaway,delivery", DeliveryMinAmount = 30, PackingFee = 1,
                MonthlySales = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            db.Stores.Add(store);
        }
        else if (!await db.Categories.AnyAsync(c => c.StoreId == store.Id))
        {
            store.Name = DemoStoreName;
            store.Description = DemoStoreDescription;
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
            RepairDemoStoreMetadata(merchant, store);
            await RepairDemoDiscountRulesAsync(db, store);
            await db.SaveChangesAsync();
            return;
        }

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
                Status = "inactive", UsedCount = 22, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            },
            new DiscountRule
            {
                Id = Guid.NewGuid(), Store = store, Name = "烧烤类8折", DiscountType = "discount",
                DiscountRate = 80, ApplyScope = "category", AllowStack = false,
                StartTime = DateTime.UtcNow.AddDays(-7), EndTime = DateTime.UtcNow.AddDays(30),
                Status = "inactive", UsedCount = 18, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();
    }

    private static void RepairDemoStoreMetadata(Merchant merchant, Store store)
    {
        var changed = false;

        if (IsBrokenText(merchant.Nickname))
        {
            merchant.Nickname = DemoMerchantName;
            changed = true;
        }

        if (IsBrokenText(store.Name))
        {
            store.Name = DemoStoreName;
            changed = true;
        }

        if (IsBrokenText(store.Description))
        {
            store.Description = DemoStoreDescription;
            changed = true;
        }

        if (store.StoreCode == "000001")
        {
            store.BusinessStatus = "open";
            store.DiningMode = string.IsNullOrWhiteSpace(store.DiningMode) ? "dine_in,takeaway,delivery" : store.DiningMode;
            store.BusinessHoursStart ??= new TimeOnly(17, 0);
            store.BusinessHoursEnd ??= new TimeOnly(2, 0);
            store.DeliveryMinAmount ??= 30;
            store.PackingFee ??= 1;
            changed = true;
        }

        if (changed)
        {
            merchant.UpdatedAt = DateTime.UtcNow;
            store.UpdatedAt = DateTime.UtcNow;
        }
    }

    private static async Task RepairDemoDiscountRulesAsync(ByteBiteDbContext db, Store store)
    {
        if (store.StoreCode != "000001") return;

        var now = DateTime.UtcNow;
        var rules = await db.DiscountRules
            .Where(rule => rule.StoreId == store.Id && rule.DeletedAt == null)
            .ToListAsync();

        var mainRule = rules.FirstOrDefault(rule =>
            rule.DiscountType == "full_reduction"
            && rule.ThresholdAmount == 100
            && rule.DiscountAmount == 10
            && rule.ApplyScope == "all");

        if (mainRule == null)
        {
            mainRule = new DiscountRule
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Name = "满100减10",
                DiscountType = "full_reduction",
                ThresholdAmount = 100,
                DiscountAmount = 10,
                ApplyScope = "all",
                AllowStack = false,
                StartTime = now.AddDays(-30),
                EndTime = now.AddDays(365),
                Status = "active",
                UsedCount = 0,
                CreatedAt = now,
                UpdatedAt = now
            };
            db.DiscountRules.Add(mainRule);
        }

        mainRule.Name = "满100减10";
        mainRule.DiscountType = "full_reduction";
        mainRule.ThresholdAmount = 100;
        mainRule.DiscountAmount = 10;
        mainRule.DiscountRate = null;
        mainRule.ApplyScope = "all";
        mainRule.ApplyScopeId = null;
        mainRule.AllowStack = false;
        mainRule.Status = "active";
        if (mainRule.StartTime > now) mainRule.StartTime = now.AddDays(-30);
        if (mainRule.EndTime < now) mainRule.EndTime = now.AddDays(365);
        mainRule.UpdatedAt = now;

        var deprecatedDemoRules = rules.Where(rule =>
            rule.Id != mainRule.Id
            && ((rule.DiscountType == "full_reduction" && rule.ThresholdAmount == 200 && rule.DiscountAmount == 30)
                || (rule.DiscountType == "discount" && rule.DiscountRate == 80)))
            .ToList();

        foreach (var rule in deprecatedDemoRules)
        {
            rule.Status = "inactive";
            rule.UpdatedAt = now;
        }

        if (deprecatedDemoRules.Count == 0) return;

        var deprecatedRuleIds = deprecatedDemoRules.Select(rule => rule.Id).ToList();
        var activeStatuses = new[] { "pending", "accepted", "preparing", "ready" };
        var affectedOrders = await db.Orders
            .Where(order => order.StoreId == store.Id
                && activeStatuses.Contains(order.Status)
                && order.DiscountRuleId.HasValue
                && deprecatedRuleIds.Contains(order.DiscountRuleId.Value))
            .ToListAsync();

        foreach (var order in affectedOrders)
        {
            var discountAmount = order.TotalAmount >= 100 ? 10 : 0;
            order.DiscountAmount = discountAmount;
            order.ActualAmount = Math.Max(0, order.TotalAmount - discountAmount + order.PackingFee);
            order.DiscountRuleId = discountAmount > 0 ? mainRule.Id : null;
            order.UpdatedAt = now;
        }
    }

    private static bool IsBrokenText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return value.Contains('?')
            || value.Contains('\uFFFD')
            || value.Contains("锛")
            || value.Contains("閲嶅簡")
            || value.Contains("姝ｅ畻")
            || value.Contains("婕旂ず");
    }
}

file class OrderSeeder : IDataSeeder
{
    public int Order => 50;

    public async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();

        var merchant = await db.Merchants.FirstOrDefaultAsync(m => m.Phone == "18523978013");
        if (merchant == null) return;

        var store = await db.Stores.FirstOrDefaultAsync(s => s.MerchantId == merchant.Id);
        if (store == null) return;

        if (await db.Orders.AnyAsync(o => o.StoreId == store.Id)) return;

        var now = DateTime.UtcNow;

        AddCompletedOrder(db, store.Id, "20260514001", "A1B2", "dine_in", 86, 0, 86, 0, now.AddDays(-7).AddHours(18));
        AddCompletedOrder(db, store.Id, "20260514002", "C3D4", "takeaway", 42, 0, 43, 1, now.AddDays(-7).AddHours(19));
        AddCompletedOrder(db, store.Id, "20260514003", "E5F6", "dine_in", 128, 10, 118, 0, now.AddDays(-7).AddHours(20));
        AddCompletedOrder(db, store.Id, "20260516001", "G7H8", "dine_in", 56, 0, 56, 0, now.AddDays(-5).AddHours(18.5));
        AddCompletedOrder(db, store.Id, "20260516002", "I9J0", "delivery", 88, 0, 89, 1, now.AddDays(-5).AddHours(19.25));
        AddCompletedOrder(db, store.Id, "20260518001", "K1L2", "dine_in", 168, 10, 158, 0, now.AddDays(-3).AddHours(19));
        AddCompletedOrder(db, store.Id, "20260518002", "M3N4", "takeaway", 35, 0, 36, 1, now.AddDays(-3).AddHours(20));
        AddCompletedOrder(db, store.Id, "20260518003", "O5P6", "dine_in", 72, 0, 72, 0, now.AddDays(-3).AddHours(21));
        AddCompletedOrder(db, store.Id, "20260520001", "Q7R8", "dine_in", 96, 0, 96, 0, now.AddDays(-1).AddHours(18));
        AddCompletedOrder(db, store.Id, "20260520002", "S9T0", "delivery", 128, 10, 119, 1, now.AddDays(-1).AddHours(19));
        AddCompletedOrder(db, store.Id, "20260520003", "U1V2", "dine_in", 45, 0, 45, 0, now.AddDays(-1).AddHours(20));

        // 今日订单 - 各种状态
        AddOrder(db, store.Id, "20260525001", "W3X4", "dine_in", 68, 0, 68, 0, "pending", now.AddHours(-1));
        AddOrder(db, store.Id, "20260525002", "Y5Z6", "takeaway", 52, 0, 53, 1, "accepted", now.AddMinutes(-45));
        AddOrder(db, store.Id, "20260525003", "A7B8", "dine_in", 136, 10, 126, 0, "preparing", now.AddMinutes(-30));
        AddOrder(db, store.Id, "20260525004", "C9D0", "delivery", 88, 0, 89, 1, "ready", now.AddMinutes(-15));

        await db.SaveChangesAsync();
    }

    private static void AddCompletedOrder(ByteBiteDbContext db, Guid storeId, string orderNo, string pickupCode, string diningMode, decimal total, decimal discount, decimal actual, decimal packingFee, DateTime createdAt)
    {
        AddOrder(db, storeId, orderNo, pickupCode, diningMode, total, discount, actual, packingFee, "completed", createdAt);
    }

    private static void AddOrder(ByteBiteDbContext db, Guid storeId, string orderNo, string pickupCode, string diningMode, decimal total, decimal discount, decimal actual, decimal packingFee, string status, DateTime createdAt)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(), StoreId = storeId, DeviceId = $"DEV-{pickupCode}",
            OrderNo = orderNo, PickupCodeValue = PickupCodeGenerator.FromDisplayCode(pickupCode), DiningMode = diningMode,
            TotalAmount = total, DiscountAmount = discount, ActualAmount = actual, PackingFee = packingFee,
            Status = status, CreatedAt = createdAt, UpdatedAt = createdAt,
            AcceptedAt = status != "pending" ? createdAt.AddMinutes(2) : null,
            PreparingAt = status is "preparing" or "ready" or "completed" ? createdAt.AddMinutes(5) : null,
            ReadyAt = status is "ready" or "completed" ? createdAt.AddMinutes(15) : null,
            CompletedAt = status == "completed" ? createdAt.AddMinutes(20) : null,
        };
        db.Orders.Add(order);
    }
}
