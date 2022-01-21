﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace RSSMS.DataService.Repositories
{
    public interface IBaseRepository<TEntity> where TEntity : class
    {
        TEntity Get<TKey>(TKey id);
        Task<TEntity> GetAsync<TKey>(TKey id);
        IQueryable<TEntity> Get();
        IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate);
        TEntity FirstOrDefault();
        Task<TEntity> FirstOrDefaultAsync();
        void Create(TEntity entity);
        Task CreateAsync(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        Task<int> Count();
    }
    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
    {
        protected DbContext dbContext;

        protected DbSet<TEntity> dbSet;

        public BaseRepository(DbContext dbContext)
        {
            this.dbContext = dbContext;
            this.dbSet = this.dbContext.Set<TEntity>();
        }

        public async Task<int> Count()
        {
            return await dbSet.CountAsync();
        }

        public void Create(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public async Task CreateAsync(TEntity entity)
        {
            await dbSet.AddAsync(entity);
        }

        public void Delete(TEntity entity)
        {
            dbSet.Remove(entity);
        }

        public TEntity FirstOrDefault()
        {
            return this.dbSet.FirstOrDefault();
        }

        public Task<TEntity> FirstOrDefaultAsync()
        {
            return this.dbSet.FirstOrDefaultAsync();
        }

        public TEntity Get<TKey>(TKey id)
        {
            return (TEntity)this.dbSet.Find(new object[1] { id });
        }

        public IQueryable<TEntity> Get()
        {
            return this.dbSet;
        }

        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> predicate)
        {
            return this.dbSet.Where(predicate);
        }

        public async Task<TEntity> GetAsync<TKey>(TKey id)
        {
            return await this.dbSet.FindAsync(new object[1] { id });
        }

        public void Update(TEntity entity)
        {
            //dbSet.Update(entity);

            dbSet.Attach(entity);

            dbContext.Entry(entity).State = EntityState.Modified;

            var entry = dbContext.Entry(entity);

            Type type = typeof(TEntity);
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (property.GetValue(entity, null) == null)
                {
                    entry.Property(property.Name).IsModified = false;
                }
            }
        }
    }
}
