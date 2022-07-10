using ExpMedia.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExpMediaCore.BaseRepository
{
    public class BaseRepository<T> : IBaseRepository<T>
        where T : class
    {
        private readonly DataContext _context;
        private readonly DbSet<T> _table;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public BaseRepository(DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _table = _context.Set<T>();
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task Create(T t)
        {
            await _table.AddAsync(t);
            await _context.SaveChangesAsync();
        }

        public async Task CreateRange(T t)
        {
            await _table.AddRangeAsync(t);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(object id)
        {
            var t = await GetOne(id);
            if (t != null)
            {
                _table.Remove(t);
                await _context.SaveChangesAsync();
            }
        }
        public string GetLoginUserId() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        public async Task<T> GetOne(object id)
        {
            return await _table.FindAsync(id);
        }

        public async Task Update(object id, object model)
        {
            var t = await GetOne(id);
            if (t != null)
            {
                _context.Entry(t).CurrentValues.SetValues(model);
                await _context.SaveChangesAsync();
            }
        }
    }
}
