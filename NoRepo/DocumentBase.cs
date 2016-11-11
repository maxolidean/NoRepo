using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NoRepo
{
    public abstract class DocumentBase<T> where T : DocumentBase<T>
    {
        public string id { get; set; }

        public string _docType
        {
            get
            {
                return typeof(T).Name;
            }
        }

        private static string collectionName;
        private static string partitionKey;

        private static string CollectionName
        {
            get
            {
                if (collectionName == null)
                    InitWithAttribute();

                return collectionName;
            }
        }

        private static string PartitionName
        {
            get
            {
                if (collectionName == null)
                    InitWithAttribute();

                return partitionKey;
            }
        }

        private static void InitWithAttribute()
        {
            var info = typeof(T);
            var att = info.GetCustomAttributes(false).FirstOrDefault(a => a is StorableAttribute) as StorableAttribute;
            collectionName = att.RepoName; // never null

            if (String.IsNullOrWhiteSpace(collectionName))
                throw new Exception("It is required to define a collection name for a document entity.");

            partitionKey = att.PartitionKey;  //can be null
        }

        private static IRepository<T> repo;

        private static IRepository<T> Repo
        {
            get
            {
                if (repo == null)
                {
                    repo = new DocumentDbRepo<T>((IRepository)RepoContext.Repos[CollectionName]);

                    if (repo.IsPartitioned && String.IsNullOrWhiteSpace(partitionKey))
                        throw new Exception("Partition key needs to be defined for partitioned collection.");
                }

                return repo;
            }
        }

        public static async Task<T> Get(string id)
        {
            return await Repo.Get(id);
        }

        public static async Task<T> Get(string id, string partitionKey)
        {
            return await Repo.Get(id, partitionKey);
        }

        public static async Task Create(T instance)
        {
            instance.id = await Repo.Create(instance);
        }

        // Suitable for override in subclass
        protected static async Task<string> Create2(T instance)
        {
            return await Repo.Create(instance);
        }

        public static async Task Upsert(T instance)
        {
            await Repo.Upsert(instance.id, instance);
        }

        // Suitable for override in subclass
        protected static async Task<string> Upsert2(T instance)
        {
            return await Repo.Upsert(instance.id, instance);
        }

        public static async Task<T> FindById(string id)
        {
            return await Repo.FirstOrDefault(p => p.id == id);
        }

        public static async Task Remove(string id)
        {
            await Repo.Remove(id);
        }

        protected static async Task Remove2(string id)
        {
            await Repo.Remove(id);
        }

        public static async Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate)
        {
            return await Repo.Where(AddDocTypePredicate(predicate));
        }

        public static async Task<T> First(Expression<Func<T, bool>> predicate)
        {
            return await Repo.First(AddDocTypePredicate(predicate));
        }

        public static async Task<T> FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return await Repo.FirstOrDefault(AddDocTypePredicate(predicate));
        }

        private static Expression<Func<T, bool>> AddDocTypePredicate(Expression<Func<T, bool>> predicate)
        {
            var type = typeof(T);
            var docTypeField = type.GetProperty("_docType");
            var memberExp = Expression.Property(predicate.Parameters[0], docTypeField);
            var secondPredicate = LambdaExpression.Equal(memberExp, Expression.Constant(type.Name));
            var and = LambdaExpression.AndAlso(predicate.Body, secondPredicate);
            var extendedPredicate = Expression.Lambda<Func<T, bool>>(and, predicate.Parameters);
            return extendedPredicate;
        }

        public static async Task<IEnumerable<T>> Query(string sql, IDictionary<string, object> parameters = null)
        {
            return await Repo.Query(sql, parameters);
        }

        public static async Task<IEnumerable<T>> GetAll()
        {
            return await Where(t => t._docType == typeof(T).Name);
        }

        public static async Task<IEnumerable<T>> Take(int count)
        {
            return await Repo.Take(t => t._docType == typeof(T).Name, count);
        }
    }

}
