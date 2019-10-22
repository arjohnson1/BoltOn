using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace BoltOn.Data.Elasticsearch
{
    public abstract class BaseElasticsearchRepository<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        private readonly IElasticClient _elasticClient;

        protected BaseElasticsearchRepository(IElasticClient elasticClient)
        {
            _elasticClient = elasticClient;
        }

        public TEntity Add(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindByAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default, params Expression<Func<TEntity, object>>[] includes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public TEntity GetById(object id)
        {
            throw new NotImplementedException();
        }

        public Task<TEntity> GetByIdAsync(object id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
