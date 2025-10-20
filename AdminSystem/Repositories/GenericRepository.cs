using AdminSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AdminSystem.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        internal CustomerEntities Context;
        internal DbSet<TEntity> DbSet;

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

            // Apply soft delete filter for specific entities
            if (typeof(TEntity) == typeof(客戶資料))
            {
                query = query.Cast<客戶資料>().Where(c => !c.是否已刪除).Cast<TEntity>();
            }
            else if (typeof(TEntity) == typeof(客戶聯絡人))
            {
                query = query.Cast<客戶聯絡人>().Where(c => !c.是否已刪除).Cast<TEntity>();
            }
            else if (typeof(TEntity) == typeof(客戶銀行資訊))
            {
                query = query.Cast<客戶銀行資訊>().Where(c => !c.是否已刪除).Cast<TEntity>();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            query = query.AsNoTracking();

            if (orderBy != null)
            {
                return orderBy(query);
            }

            return query;
        }

        public virtual TEntity GetById(object id)
        {
            return DbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            DbSet.Add(entity);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            DbSet.Update(entityToUpdate);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = DbSet.Find(id);
            if (entityToDelete != null)
            {
                // Set 是否已刪除 to true for soft delete
                var property = Context.Entry(entityToDelete).Property("是否已刪除");
                if (property != null)
                {
                    property.CurrentValue = true;
                }
            }
        }
    }
}