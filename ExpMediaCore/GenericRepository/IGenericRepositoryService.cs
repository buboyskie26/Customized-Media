using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.GenericRepository
{
    public interface IGenericRepositoryService<T> where T : class
    {
        Task<ICollection<T>> FindAll();
        Task<T> FindById(int id);
        Task<bool> isExists(int id);
        Task<bool> Create(T entity);
        Task Update(T entity);
        Task<bool> Save();
    }
}
