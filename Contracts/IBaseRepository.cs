using System;
using System.Linq.Expressions;

namespace ClinicBooking.API.Contracts;

public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);

    Task<List<T>> GetAllAsync();
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);

    void Update(T entity);
    Task Delete(Guid id);

}
