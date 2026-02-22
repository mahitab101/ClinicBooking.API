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

    public async Task Delete(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity == null)
        {
            throw new ArgumentException($"Entity with id {id} not found.");
        }
        if (entity is SoftDeleteEntity softDelete)
        {
            softDelete.IsDeleted = true;
            _dbSet.Update(entity);
        }
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var entity = await _dbSet.FindAsync(id);
        return entity;
    }
    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
    public async Task<List<T>> FindAsync(
            Expression<Func<T, bool>> predicate)
            => await _dbSet.Where(predicate).ToListAsync();

}
