using ExpMedia.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpMediaCore.Base
{
    public class EntityBaseRepository<T> : IEntityBaseRepository<T> where T : class
    {
        private readonly DataContext _context;

        public EntityBaseRepository(DataContext context)
        {
            _context = context;
        }

        public async Task AddEntityAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public Task DeleteEntityAsync(int id)
        {
            /*            var entityId = await _context.Set<T>().FirstOrDefaultAsync(i => i.Id == id);

                        EntityEntry entityEntry = _context.Entry<T>(entityId);
                        entityEntry.State = EntityState.Deleted;
                        await _context.SaveChangesAsync();*/
            throw new NotImplementedException();

        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public Task<T> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task UpdateEntityAsync(T entity, int id)
        {
            EntityEntry entityEntry = _context.Entry<T>(entity);
            entityEntry.State = EntityState.Modified;

            await _context.SaveChangesAsync();
        }


    }
}
