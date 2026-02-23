using System;
using System.Linq.Expressions;

namespace ClinicBooking.API.Contracts;

public interface IBaseRepository<T> where T : class
{
    Task<T> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task<List<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    Task<bool> Delete(Guid id);
    IQueryable<T> Query();

}
