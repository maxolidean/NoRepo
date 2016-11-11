using System;

namespace NoRepo
{
    [AttributeUsage(AttributeTargets.Class)]
    public class StorableAttribute : Attribute
    {
        public readonly string RepoName;
        private string partitionKey;

        public StorableAttribute(string repoName)
        {
            this.RepoName = repoName;
        }

        public StorableAttribute(string repoName, string partitionKey)
        {
            this.RepoName = repoName;
            this.partitionKey = partitionKey;
        }

        public string PartitionKey
        {
            get
            {
                return partitionKey;
            }

            set
            {
                partitionKey = value;
            }
        }
    }
}
