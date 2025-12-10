// File: Infrastructure/Repositories/Repository.cs

using AdminSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AdminSystem.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
    }

    public virtual IQueryable<TEntity> Get(
        Expression<Func<TEntity, bool>>? filter = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        string includeProperties = "")
    {
        IQueryable<TEntity> query = DbSet;

        // Auto soft-delete filter
        if (typeof(TEntity).GetProperty("是否已刪除") != null)
        {
            query = query.Where(e => EF.Property<bool>(e, "是否已刪除") == false);
        }

        if (filter != null)
            query = query.Where(filter);

        foreach (var include in includeProperties
                     .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                     .Select(i => i.Trim()))
        {
            query = query.Include(include);
        }

        query = query.AsNoTracking();

        return orderBy != null ? orderBy(query) : query;
    }

    public virtual Task<TEntity?> GetByIdAsync(object id) => DbSet.FindAsync(id).AsTask();

    public Task<List<TEntity>> GetAllAsync() => Get().ToListAsync();

    public Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter)
        => Get(filter).FirstOrDefaultAsync();

    public async Task InsertAsync(TEntity entity)
    {
        await DbSet.AddAsync(entity);
    }

    // FIXED: Now async and consistent!
    public virtual Task UpdateAsync(TEntity entity)
    {
        // Attach and mark as modified
        DbSet.Update(entity);
        // Or: Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    // FIXED: SoftDeleteAsync should be public + async
    public virtual async Task SoftDeleteAsync(object id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity == null) return;

        var entry = Context.Entry(entity);
        var prop = entry.Property("是否已刪除");

        if (prop?.Metadata.PropertyInfo?.PropertyType == typeof(bool))
        {
            prop.CurrentValue = true;
        }
    }

    public virtual Task HardDeleteAsync(object id)
    {
        return DbSet.Where(e => EF.Property<object>(e, "Id") == id)
                      .ExecuteDeleteAsync();
    }

    // Optional: Keep sync versions if legacy code needs them
    public virtual void Update(TEntity entity) => DbSet.Update(entity);
    public virtual void Delete(object id) => HardDeleteAsync(id).Wait();
}