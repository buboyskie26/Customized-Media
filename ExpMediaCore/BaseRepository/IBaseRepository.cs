using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpMediaCore.BaseRepository
{
    public interface IBaseRepository<T>
    {
        Task Create(T t);
        Task CreateRange(T t);
        Task Update(object id, object model);
        Task<T> GetOne(object id);
        Task Delete(object id);
        string GetLoginUserId();
    }
}
