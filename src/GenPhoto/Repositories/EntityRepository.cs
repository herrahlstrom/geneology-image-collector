using GenPhoto.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace GenPhoto.Repositories
{
    public class EntityRepository<TEntity> : IDisposable where TEntity : class
    {
        readonly AppDbContext m_db;
        readonly DbSet<TEntity> m_dbSet;
        readonly ILogger m_logger;

        internal EntityRepository(IDbContextFactory<AppDbContext> dbFactory, ILogger<EntityRepository<TEntity>> logger)
        {
            m_logger = logger;
            m_db = dbFactory.CreateDbContext();
            m_dbSet = m_db.Set<TEntity>();
        }

        public async Task<bool> AddOrUpdateEntityAsync(
            Func<TEntity> addAction,
            Action<TEntity> updateAction,
            params object[] keyValues)
        {
            bool updated = false;
            if(await m_dbSet.FindAsync(keyValues) is { } entity)
            {
                updateAction.Invoke(entity);
                updated = true;
            }
            else
            {
                entity = addAction();
                if(entity != null)
                {
                    m_dbSet.Add(entity);
                }
            }
            var result = await m_db.SaveChangesAsync();

            if(updated)
            {
                m_logger.LogTrace("Updated {count} entities in DB set {set}", result, typeof(TEntity).Name);
            }
            else
            {
                m_logger.LogTrace("Add {count} entities to DB set {set}", result, typeof(TEntity).Name);
            }

            return result > 0;
        }

        public void Dispose() { m_db.Dispose(); }

        public async Task<IList<TEntity>> GetEntitiesAsync() { return await m_dbSet.ToListAsync(); }

        public async Task<TEntity?> GetEntryAsync(params object[] keyValues)
        { return await m_dbSet.FindAsync(keyValues); }

        public async Task<int> RemoveEntryAsync(params object[] keyValues)
        {
            if(await m_dbSet.FindAsync(keyValues) is { } entity)
            {
                m_dbSet.Remove(entity);
                int result = await m_db.SaveChangesAsync();

                m_logger.LogTrace("Remove {count} entities from DB set {set}", result, typeof(TEntity).Name);

                return result;
            }

            return 0;
        }

        public async Task<int> RemoveWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await m_dbSet.Where(predicate).ToListAsync();
            if(entities.Count == 0)
            {
                return 0;
            }

            foreach(var entity in entities)
            {
                m_dbSet.Remove(entity);
            }

            var result = await m_db.SaveChangesAsync();

            m_logger.LogTrace("Remove {count} entities from DB set {set}", result, typeof(TEntity).Name);

            return result;
        }
    }

    public class EntityRepositoryFactory
    {
        readonly ILoggerFactory m_loggerFactory;
        private IDbContextFactory<AppDbContext> m_dbFactory;

        public EntityRepositoryFactory(IDbContextFactory<AppDbContext> dbFactory, ILoggerFactory loggerFactory)
        {
            m_loggerFactory = loggerFactory;
            m_dbFactory = dbFactory;
        }

        public EntityRepository<TEntity> Create<TEntity>() where TEntity : class
        {
            return new EntityRepository<TEntity>(m_dbFactory, m_loggerFactory.CreateLogger<EntityRepository<TEntity>>());
        }
    }
}