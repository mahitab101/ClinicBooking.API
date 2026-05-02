using System;
using System.Linq.Expressions;
using ClinicBooking.API.Common;
using ClinicBooking.API.Contracts;
using ClinicBooking.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicBooking.API.Repository;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
        _dbSet = _dbContext.Set<T>();
    }
    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task<bool> Delete(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null) return false;
        if (entity is SoftDeleteEntity softDelete)
        {
            softDelete.IsDeleted = true;
            _dbSet.Update(entity);
        }
        return true;
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.AsQueryable().ToListAsync();
    }

    public async Task<T> GetByIdAsync(Guid id)
    {
        return await _dbSet.AsQueryable()
            .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
    }
    public void Update(T entity) => _dbSet.Update(entity);

    public async Task<List<T>> GetByConditionAsync(
            Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

    public IQueryable<T> Query() => _dbSet.AsNoTracking();
}
