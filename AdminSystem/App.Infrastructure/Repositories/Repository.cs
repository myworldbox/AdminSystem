using AdminSystem.Application.ViewModels;
using AdminSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AdminSystem.Infrastructure.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly AppDbContext Context;
        protected readonly DbSet<TEntity> DbSet;

        public Repository(AppDbContext context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>>? filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy,
            string includeProperties = "")
        {
            IQueryable<TEntity> query = DbSet;

            // Apply soft delete filter dynamically
            if (typeof(TEntity).GetProperty("是否已刪除") != null)
            {
                var param = Expression.Parameter(typeof(TEntity), "x");
                var prop = Expression.Property(param, "是否已刪除");
                var notDeleted = Expression.Lambda<Func<TEntity, bool>>(
                    Expression.Equal(prop, Expression.Constant(false)), param);
                query = query.Where(notDeleted);
            }

            if (filter != null) query = query.Where(filter);

            foreach (var include in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                query = query.Include(include.Trim());

            return orderBy != null ? orderBy(query.AsNoTracking()) : query.AsNoTracking();
        }

        public virtual TEntity GetById(object id) => DbSet.Find(id);

        public virtual void Insert(TEntity entity) => DbSet.Add(entity);

        public virtual void Update(TEntity entity) => DbSet.Update(entity);

        public virtual void Delete(object id)
        {
            var entity = DbSet.Find(id);
            if (entity == null) return;

            // Mark root entity
            var prop = Context.Entry(entity).Property("是否已刪除");
            if (prop != null) prop.CurrentValue = true;

            // Example: if TEntity has a collection navigation "Children"
            var navigation = Context.Entry(entity).Collections;
            foreach (var nav in navigation)
            {
                nav.Load(); // ensure related entities are loaded
                foreach (var child in (IEnumerable<object>)nav.CurrentValue!)
                {
                    var childProp = Context.Entry(child).Property("是否已刪除");
                    if (childProp != null) childProp.CurrentValue = true;
                }
            }
        }

    }
}
