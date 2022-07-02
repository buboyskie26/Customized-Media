using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMediaCore.Base
{
    public interface IEntityBaseRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task AddEntityAsync(T entity);
        Task DeleteEntityAsync(int id);
        Task UpdateEntityAsync(T entity, int id);
    }
}
