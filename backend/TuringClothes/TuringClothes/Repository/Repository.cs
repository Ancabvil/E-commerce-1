
using Microsoft.EntityFrameworkCore;
using TuringClothes.Database;

namespace TuringClothes.Repository
{
    public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class
    {
        protected MyDatabase _myDatabase { get; init; }

        public Repository(MyDatabase myDatabase)
        {
            _myDatabase = myDatabase;
        }

        public async Task<ICollection<TEntity>> GetAllAsync()
        {
            return await _myDatabase.Set<TEntity>().ToArrayAsync();
        }
        public Task<TEntity> DeleteAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistAsync(TId id)
        {
            throw new NotImplementedException();
        }

        

        public async Task<TEntity> GetByIdAsync(TId id)
        {
            return await _myDatabase.Set<TEntity>().FindAsync(id);
        }

        public IQueryable<TEntity> GetQueryable(bool asNoTracking = true)
        {
            DbSet<TEntity> entities = _myDatabase.Set<TEntity>();
            return asNoTracking ? entities.AsNoTracking() : entities;
        }

        public Task<TEntity> InsertAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }

       

        public async Task UpdateAsync(TEntity entity)
        {
            _myDatabase.Set<TEntity>().Update(entity);
            await _myDatabase.SaveChangesAsync();
        }

        Task<TEntity> IRepository<TEntity, TId>.UpdateAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
