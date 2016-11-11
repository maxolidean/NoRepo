using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NoRepo
{
    public class DocumentDbRepo : IRepository
    {
        private DocumentCollection collection { get; set; }
        private Uri collectionUri { get; set; }
        private DocumentClient DocumentClient { get; set; }
        private string dbName { get; set; }
        private string collectionName { get; set; }

        public DocumentDbRepo(string dbName, string collectionName, DocumentClient client)
        {
            if (String.IsNullOrWhiteSpace(dbName))
                throw new ArgumentException("dbName");

            if (String.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("collectionName");

            if (client == null)
                throw new ArgumentNullException("client", "A DocumentClient instance is required.");

            this.dbName = dbName;
            this.collectionName = collectionName;
            this.DocumentClient = client;
            collectionUri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);
            collection = DocumentClient.ReadDocumentCollectionAsync(collectionUri).Result;
        }

        public DocumentDbRepo(string endpoint, string key, string dbName, string collectionName)
        {
            if (String.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("endpoint");

            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (!RepoContext.DocumentClients.ContainsKey(endpoint))
                RepoContext.DocumentClients.Add(endpoint, RepoContext.CreateClient(endpoint, key));

            this.dbName = dbName;
            this.collectionName = collectionName;
            this.DocumentClient = RepoContext.DocumentClients[endpoint];
            collectionUri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);
            collection = DocumentClient.ReadDocumentCollectionAsync(collectionUri).Result;
        }

        public bool IsPartitioned
        {
            get { return collection.PartitionKey.Paths.Count > 0; }
        }

        public async Task<string> Create<T>(T instance) where T : class
        {
            var resp = await DocumentClient.CreateDocumentAsync(collectionUri, instance);
            return resp.Resource.Id;
        }

        public async Task<T> Get<T>(string id) where T : class
        {
            var uri = UriFactory.CreateDocumentUri(dbName, collectionName, id);

            RequestOptions options = null;

            var resp = await DocumentClient.ReadDocumentAsync(uri, options);
            return JsonConvert.DeserializeObject<T>(resp.Resource.ToString());
        }

        public async Task<T> Get<T>(string id, string partitionKey) where T : class
        {
            var uri = UriFactory.CreateDocumentUri(dbName, collectionName, id);

            RequestOptions options = null;
            if (this.IsPartitioned)
                options = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

            var resp = await DocumentClient.ReadDocumentAsync(uri, options);
            return JsonConvert.DeserializeObject<T>(resp.Resource.ToString());
        }

        public async Task<IEnumerable<T>> Where<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var query = DocumentClient.CreateDocumentQuery<T>(collectionUri).Where(predicate).AsDocumentQuery();
            return await query.ExecuteNextAsync<T>();
        }

        public async Task<IEnumerable<T>> Take<T>(Expression<Func<T, bool>> predicate, int count) where T : class
        {
            var query = DocumentClient.CreateDocumentQuery<T>(collectionUri).Where(predicate).Take(count).AsDocumentQuery();
            return await query.ExecuteNextAsync<T>();
        }


        public async Task<T> FirstOrDefault<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var query = DocumentClient.CreateDocumentQuery<T>(collectionUri).Where(predicate).AsDocumentQuery();
            var result = await query.ExecuteNextAsync<T>();
            return result.FirstOrDefault<T>();
        }
        public async Task<T> First<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var query = DocumentClient.CreateDocumentQuery<T>(collectionUri).Where(predicate).AsDocumentQuery();
            var result = await query.ExecuteNextAsync<T>();
            return result.First<T>();
        }

        public async Task Remove(string id)
        {
            var uri = UriFactory.CreateDocumentUri(dbName, collectionName, id);
            await DocumentClient.DeleteDocumentAsync(uri);
        }

        public async Task Remove(string id, string partitionKey)
        {
            var uri = UriFactory.CreateDocumentUri(dbName, collectionName, id);

            RequestOptions options = null;
            if (this.IsPartitioned)
                options = new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) };

            await DocumentClient.DeleteDocumentAsync(uri, options);
        }

        public async Task<string> Upsert<T>(string id, T instance) where T : class
        {
            var uri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);

            var resp = await DocumentClient.UpsertDocumentAsync(uri, instance);
            return resp.Resource.Id;
        }

        public async Task<string> Upsert<T>(string id, string partitionKey, T instance) where T : class
        {
            var uri = UriFactory.CreateDocumentCollectionUri(dbName, collectionName);

            RequestOptions options = null;

            if (this.IsPartitioned)
                options = new RequestOptions() { PartitionKey = new PartitionKey(id) };

            var resp = await DocumentClient.UpsertDocumentAsync(uri, instance, options);
            return resp.Resource.Id;
        }


        public async Task<IEnumerable<T>> Query<T>(string queryExpression, IDictionary<string, object> parameters = null) where T : class
        {
            var paramsCol = new SqlParameterCollection();

            if (parameters != null)
                foreach (var parameter in parameters)
                    paramsCol.Add(new SqlParameter(parameter.Key, parameter.Value));


            var querySpec = new SqlQuerySpec(queryExpression, paramsCol);

            var query = DocumentClient.CreateDocumentQuery<T>(collectionUri, querySpec, null).AsDocumentQuery<T>();

            var batches = new List<IEnumerable<T>>();

            do
            {
                batches.Add(await query.ExecuteNextAsync<T>());
            }
            while (query.HasMoreResults);

            return batches.SelectMany(b => b);
        }

        public async Task<IEnumerable<dynamic>> Query(string queryExpression, IDictionary<string, object> parameters = null)
        {
            var paramsCol = new SqlParameterCollection();

            if (parameters != null)
                foreach (var parameter in parameters)
                    paramsCol.Add(new SqlParameter(parameter.Key, parameter.Value));


            var querySpec = new SqlQuerySpec(queryExpression, paramsCol);

            var query = DocumentClient.CreateDocumentQuery(collectionUri, querySpec, null).AsDocumentQuery();

            var batches = new List<IEnumerable<dynamic>>();

            do
            {
                batches.Add(await query.ExecuteNextAsync());
            }
            while (query.HasMoreResults);

            return batches.SelectMany(b => b);
        }

    }

    public sealed class DocumentDbRepo<T> : IRepository<T> where T : class
    {
        private IRepository Repo { get; set; }

        public DocumentDbRepo(IRepository repo)
        {
            Repo = repo;
        }

        public DocumentDbRepo(string dbName, string collectionName, DocumentClient client)
        {
            Repo = new DocumentDbRepo(dbName, collectionName, client);
        }
        public DocumentDbRepo(string endpoint, string key, string dbName, string collectionName)
        {
            Repo = new DocumentDbRepo(endpoint, key, dbName, collectionName);
        }

        public bool IsPartitioned
        {
            get { return Repo.IsPartitioned; }
        }

        public async Task<string> Create(T instance)
        {
            return await Repo.Create<T>(instance);
        }

        public async Task<T> Get(string id)
        {
            return await Repo.Get<T>(id);
        }

        public async Task<T> Get(string id, string partitionKey)
        {
            return await Repo.Get<T>(id, partitionKey);
        }

        public async Task<IEnumerable<T>> Where(Expression<Func<T, bool>> predicate)
        {
            return await Repo.Where<T>(predicate);
        }

        public async Task<IEnumerable<T>> Take(Expression<Func<T, bool>> predicate, int count)
        {
            return await Repo.Take<T>(predicate, count);
        }

        public async Task<T> FirstOrDefault(Expression<Func<T, bool>> predicate)
        {
            return await Repo.FirstOrDefault<T>(predicate);
        }

        public async Task<T> First(Expression<Func<T, bool>> predicate)
        {
            return await Repo.First<T>(predicate);
        }

        public async Task Remove(string id)
        {
            await Repo.Remove(id);
        }
        public async Task Remove(string id, string partitionKey)
        {
            await Repo.Remove(id, partitionKey);
        }
        public async Task<string> Upsert(string id, T instance)
        {
            return await Repo.Upsert<T>(id, instance);
        }

        public async Task<string> Upsert(string id, string partitionKey, T instance)
        {
            return await Repo.Upsert<T>(id, partitionKey, instance);
        }

        public async Task<IEnumerable<T>> Query(string queryExpression, IDictionary<string, object> parameters = null)
        {
            return await Repo.Query<T>(queryExpression, parameters);
        }

    }

}
