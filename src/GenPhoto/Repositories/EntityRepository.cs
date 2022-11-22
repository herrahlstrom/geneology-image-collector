using GenPhoto.Data;
using Microsoft.EntityFrameworkCore;

namespace GenPhoto.Repositories
{
    public class EntityRepository<TEntity> : IDisposable where TEntity : class
    {
        private AppDbContext m_db;
        private DbSet<TEntity> m_dbSet;

        internal EntityRepository(IDbContextFactory<AppDbContext> dbFactory)
        {
            m_db = dbFactory.CreateDbContext();
            m_dbSet = m_db.Set<TEntity>();
        }

        public async Task<bool> AddOrUpdateEntityAsync(Func<TEntity> addAction, Action<TEntity> updateAction, params object[] keyValues)
        {
            if (await m_dbSet.FindAsync(keyValues) is { } entity)
            {
                updateAction.Invoke(entity);
            }
            else
            {
                entity = addAction();
                if (entity != null)
                {
                    m_dbSet.Add(entity);
                }
            }
            return await m_db.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            m_db.Dispose();
        }

        public async Task<IList<TEntity>> GetEntitiesAsync()
        {
            return await m_dbSet.ToListAsync();
        }

        public async Task<TEntity?> GetEntityAsync(params object[] keyValues)
        {
            return await m_dbSet.FindAsync(keyValues);
        }

        public async Task<bool> RemoveEntityAsync(params object[] keyValues)
        {
            if (await m_dbSet.FindAsync(keyValues) is { } entity)
            {
                m_dbSet.Remove(entity);
                return await m_db.SaveChangesAsync() > 0;
            }

            return false;
        }

        public async Task<bool> UpdateEntityAsync(Action<TEntity> updateAction, params object[] keyValues)
        {
            if (await m_dbSet.FindAsync(keyValues) is { } entity)
            {
                updateAction.Invoke(entity);
            }
            return await m_db.SaveChangesAsync() > 0;
        }
    }

    public class EntityRepositoryFactory
    {
        private IDbContextFactory<AppDbContext> m_dbFactory;

        public EntityRepositoryFactory(IDbContextFactory<AppDbContext> dbFactory)
        {
            m_dbFactory = dbFactory;
        }

        public EntityRepository<TEntity> Create<TEntity>() where TEntity : class
        {
            return new EntityRepository<TEntity>(m_dbFactory);
        }
    }
}