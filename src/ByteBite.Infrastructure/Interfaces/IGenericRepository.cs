using ByteBite.Infrastructure.Persistence.Entities;

namespace ByteBite.Infrastructure.Interfaces;

/// <summary>
/// 通用仓储接口 - 定义基础CRUD操作
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>根据ID获取实体</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>获取所有实体</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);

    /// <summary>添加实体</summary>
    Task AddAsync(T entity, CancellationToken ct = default);

    /// <summary>更新实体</summary>
    void Update(T entity);

    /// <summary>删除实体</summary>
    void Remove(T entity);

    /// <summary>保存所有变更到数据库</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
