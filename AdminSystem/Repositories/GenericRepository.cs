using AdminSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AdminSystem.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        protected readonly CustomerEntities Context;
        protected readonly DbSet<TEntity> DbSet;

        public GenericRepository(CustomerEntities context)
        {
            Context = context;
            DbSet = context.Set<TEntity>();
        }

        public virtual IQueryable<TEntity> Get(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
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

            var prop = Context.Entry(entity).Property("是否已刪除");
            if (prop != null) prop.CurrentValue = true;
        }
    }
}
