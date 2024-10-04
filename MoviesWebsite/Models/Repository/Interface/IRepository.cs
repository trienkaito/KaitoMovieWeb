namespace MoviesWebsite.Models.Repository.Interface;
public interface IRepository<T> where T : class
{
    Task<T>? FindIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task DeleteAsync(T entity);
    Task<IEnumerable<T>> GetAllAsync();
}