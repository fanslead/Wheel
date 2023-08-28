﻿using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Wheel.Domain;

namespace Wheel.EntityFrameworkCore
{
    public class EFBasicRepository<TEntity, TKey> : IBasicRepository<TEntity, TKey> where TEntity : class
    {
        private readonly WheelDbContext _dbContext;

        public EFBasicRepository(WheelDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> InsertAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<int> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<TEntity>().Update(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<int> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            _dbContext.Set<TEntity>().Remove(entity);
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        public async Task<TEntity?> FindAsync(TKey id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>().FindAsync(id, cancellationToken);
        }
        public async Task<List<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<TEntity>().Where(predicate).ToListAsync(cancellationToken);
        }
        public async Task<(List<TEntity> items, long total)> GetPageListAsync(Expression<Func<TEntity, bool>> predicate, int skip, int take, string orderby = "Id", CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Set<TEntity>().Where(predicate);
            var total = await query.LongCountAsync(cancellationToken);
            var items = await query.OrderBy(orderby)
                .Skip(skip).Take(take)
                .ToListAsync(cancellationToken);
            return (items, total);
        }
    }
}