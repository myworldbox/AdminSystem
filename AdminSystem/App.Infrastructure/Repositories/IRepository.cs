// File: Infrastructure/Repositories/IRepository.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AdminSystem.Infrastructure.Repositories;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// 取得資料（自動排除已軟刪除的資料 + AsNoTracking）
    /// </summary>
    IQueryable<TEntity> Get(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "");

    /// <summary>
    /// 依主鍵取得單筆（非追蹤）
    /// </summary>
    Task<TEntity?> GetByIdAsync(object id);

    /// <summary>
    /// 取得所有符合條件的資料（已套用軟刪除過濾）
    /// </summary>
    Task<List<TEntity>> GetAllAsync();

    /// <summary>
    /// 取得第一筆符合條件的資料
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter);

    /// <summary>
    /// 新增（非同步）
    /// </summary>
    Task InsertAsync(TEntity entity);

    /// <summary>
    /// 更新（同步即可，EF Core 會在 SaveChanges 時處理）
    /// </summary>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// 軟刪除（只標記 是否已刪除 = true）
    /// </summary>
    Task SoftDeleteAsync(object id);

    /// <summary>
    /// 真正從資料庫刪除（慎用！）
    /// </summary>
    Task HardDeleteAsync(object id);
}