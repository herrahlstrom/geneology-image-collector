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

        private void LogRemovedEntries(int numberOfAffectedEntries)
        {
            string entityTypeName = typeof(TEntity).Name;

            switch (numberOfAffectedEntries)
            {
                case 0:
                    m_logger.LogTrace("No entities removed from db set {set}", entityTypeName);
                    break;
                case 1:
                    m_logger.LogTrace("Remove single entity from DB set {set}", entityTypeName);
                    break;
                default:
                    m_logger.LogTrace("Remove {count} entities from DB set {set}", numberOfAffectedEntries, entityTypeName);
                    break;
            }
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

        public async Task<TEntity?> GetEntryAsync(params object[] keyValues)
        {
            return await m_dbSet.FindAsync(keyValues);
        }

        public async Task<int> RemoveEntryAsync(params object[] keyValues)
        {
            if (await m_dbSet.FindAsync(keyValues) is { } entity)
            {
                m_dbSet.Remove(entity);
                int result = await m_db.SaveChangesAsync();

                LogRemovedEntries(result);

                return result;
            }

            return 0;
        }

        public async Task<int> RemoveWhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = await m_dbSet.Where(predicate).ToListAsync();
            if (entities.Count == 0)
            {
                return 0;
            }

            foreach (var entity in entities)
            {
                m_dbSet.Remove(entity);
            }

            var result = await m_db.SaveChangesAsync();

            LogRemovedEntries(result);

            return result;
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