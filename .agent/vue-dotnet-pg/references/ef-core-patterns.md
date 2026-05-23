# EF Core 常用模式与踩坑

本文件按需加载，不是常驻协议。

## DbContext 配置

```csharp
public class ByteBiteDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public ByteBiteDbContext(DbContextOptions<ByteBiteDbContext> options, ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("CreatedBy").CurrentValue = _currentUserService.UserId;
                    break;
                case EntityState.Modified:
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                    entry.Property("UpdatedBy").CurrentValue = _currentUserService.UserId;
                    break;
            }
        }

        return base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ByteBiteDbContext).Assembly);

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entity.Name).ToTable(entity.Name.Replace("ByteBite.", ""), "public");
        }
    }
}
```

## 实体配置模式

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasColumnName("id")
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .HasDatabaseName("uq_users_email")
            .IsUnique();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("now()")
            .ValueGeneratedOnAdd();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamptz");

        builder.Property(u => u.DeletedAt)
            .HasColumnName("deleted_at")
            .HasColumnType("timestamptz");

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
```

## 值对象映射

```csharp
builder.OwnsOne(u => u.Address, a =>
{
    a.Property(p => p.Province).HasColumnName("province").HasMaxLength(50);
    a.Property(p => p.City).HasColumnName("city").HasMaxLength(50);
    a.Property(p => p.Detail).HasColumnName("detail").HasMaxLength(200);
});
```

## 枚举存储

```csharp
builder.Property(u => u.Status)
    .HasColumnName("status")
    .HasConversion<string>()
    .HasMaxLength(20);
```

## 分页查询模式

```csharp
public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
    this IQueryable<T> query, int pageIndex, int pageSize, CancellationToken ct)
{
    var totalCount = await query.CountAsync(ct);
    var items = await query
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(ct);

    return new PagedResult<T>(items, totalCount, pageIndex, pageSize);
}
```

## 常见踩坑

| 坑 | 说明 | 解法 |
|---|---|---|
| N+1 查询 | 循环中访问导航属性 | 使用 `Include` / `ThenInclude` 预加载，或 `AsNoTracking` + 投影 |
| 软删除 + 导航属性 | 查询过滤器不影响导航属性加载 | 手动过滤或使用 `IgnoreQueryFilters` |
| DateTime 时区 | `DateTime` 默认无时区 | 统一使用 `DateTime.UtcNow`，数据库列用 `timestamptz` |
| decimal 精度 | EF Core 默认映射 `decimal` 为无精度限制 | 显式指定 `.HasPrecision(18, 2)` |
| 迁移冲突 | 多人并行迁移 | 迁移前先 pull，合并冲突后重新生成 |
| AsNoTracking 滥用 | 只读查询用 `AsNoTracking` 提升性能，但写操作不能省 | 只在纯查询场景使用 |
| 大事务 | 单次 SaveChanges 涉及过多实体 | 拆分批次，每批 100-200 条 |
| 字符串无长度 | PostgreSQL 默认无限制文本 | 始终指定 `HasMaxLength()` |
