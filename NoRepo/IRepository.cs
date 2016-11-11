using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NoRepo
{
    public interface IRepository
    {
        bool IsPartitioned
        {
            get;
        }

        Task<string> Create<T>(T instance) where T : class;

        Task<string> Upsert<T>(string id, T instance) where T : class;

        Task<string> Upsert<T>(string id, string partitionKey, T instance) where T : class;

        Task Remove(string id);

        Task Remove(string id, string partitionKey);

        Task<T> Get<T>(string id) where T : class;

        Task<T> Get<T>(string id, string partitionKey) where T : class;

        Task<IEnumerable<T>> Where<T>(Expression<Func<T, bool>> predicate) where T : class;

        Task<IEnumerable<T>> Take<T>(Expression<Func<T, bool>> predicate, int count) where T : class;

        Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class;

        Task<T> First<T>(Expression<Func<T, bool>> predicate) where T : class;

        Task<IEnumerable<T>> Query<T>(string queryExpression, IDictionary<string, object> parameters = null) where T : class;

        Task<IEnumerable<dynamic>> Query(string queryExpression, IDictionary<string, object> parameters = null);
    }

    public interface IRepository<T> where T : class
    {
        bool IsPartitioned
        {
            get;
        }

        Task<string> Create(T instance);

        Task<string> Upsert(string id, T instance);

        Task<string> Upsert(string id, string partitionKey, T instance);

        Task Remove(string id);

        Task Remove(string id, string partitionKey);

        Task<T> Get(string id);

        Task<T> Get(string id, string partitionKey);

        Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> Take(Expression<Func<T, bool>> predicate, int count);

        Task<T> First(Expression<Func<T, bool>> predicate);

        Task<T> FirstOrDefault(Expression<Func<T, bool>> predicate);

        Task<IEnumerable<T>> Query(string queryExpression, IDictionary<string, object> parameters = null);
    }
}
