using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoRepo
{
    public static class RepoContext
    {
        internal static Dictionary<string, IRepository> Repos { get; private set; }
        internal static Dictionary<string, DocumentClient> DocumentClients { get; set; }

        static RepoContext()
        {
            Repos = new Dictionary<string, IRepository>();
            DocumentClients = new Dictionary<string, DocumentClient>();
        }


        public static void AddRepo(string name, IRepository repo)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name");

            if (repo == null)
                throw new ArgumentNullException("repo");

            Repos.Add(name, repo);
        }

        public static void AddDocumentDbRepo(DocumentClient client, string dbName, string collectionName)
        {
            Repos.Add(collectionName, new DocumentDbRepo(dbName, collectionName, client));
        }

        public static void AddDocumentDbRepo(string endpoint, string key, string dbName, string collectionName)
        {
            if (String.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("endpoint");

            if (String.IsNullOrWhiteSpace(key))
                throw new ArgumentException("key");

            if (!DocumentClients.ContainsKey(endpoint))
                DocumentClients.Add(endpoint, CreateClient(endpoint, key));

            AddDocumentDbRepo(DocumentClients[endpoint], dbName, collectionName);
        }

        internal static DocumentClient CreateClient(string endpoint, string key)
        {
            var uri = new Uri(endpoint);
            return new DocumentClient(uri, key,
                            new ConnectionPolicy()
                            {
                                ConnectionMode = ConnectionMode.Direct,
                                ConnectionProtocol = Protocol.Tcp
                            });

        }
    }
}
