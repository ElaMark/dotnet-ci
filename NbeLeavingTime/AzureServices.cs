using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;
using NbeLeavingTime.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace NbeLeavingTime
{
    public class AzureServices
    {
        public static CloudTable GetAzureTable(string TableName)
        {
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ToString());

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference(TableName);
            table.CreateIfNotExists(new TableRequestOptions() { RetryPolicy = new ExponentialRetry() });

            return table;
        }

        public static void SaveExceptionToAzure(Post p, Exception xcp)
        {
            CloudTable table = GetAzureTable("Exceptions");
            ExceptionEntity exceptionsEntity = null;
            if (p != null)
                exceptionsEntity = new ExceptionEntity(ConfigurationManager.AppSettings["TablePartitionKey"].ToString(), Guid.NewGuid().ToString()) { ExceptionTime = DateTime.Now, Person = p.Person, Message = p.Message, ExceptionDetails = xcp.ToString() };
            else
                exceptionsEntity = new ExceptionEntity(ConfigurationManager.AppSettings["TablePartitionKey"].ToString(), Guid.NewGuid().ToString()) { ExceptionTime = DateTime.Now, Person = "unknown", Message = "unknown", ExceptionDetails = xcp.ToString() };

            TableOperation insertOperation = TableOperation.InsertOrReplace(exceptionsEntity);
            TableResult result = table.Execute(insertOperation);
        }

        public static List<Post> GetPostsFromAzure()
        {
            CloudTable table = GetAzureTable("posts");
            TableQuery<PostsEntity> query = new TableQuery<PostsEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ConfigurationManager.AppSettings["TablePartitionKey"].ToString()));

            List<Post> allPosts = new List<Post>();
            foreach (PostsEntity entity in table.ExecuteQuery(query))
            {
                allPosts.Add(new Post() { PostTime = entity.PostTime, 
                    FormattedDate = entity.PostTime.ToString("dddd dd MMMM yyyy, HH:mm:ss"),
                    Person = entity.Person, 
                    Message = entity.Message });
            }

            return allPosts;

        }

        public static void RemoveAllPostsFromAzure()
        {
            CloudTable table = GetAzureTable("posts");
            TableQuery<PostsEntity> query = new TableQuery<PostsEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ConfigurationManager.AppSettings["TablePartitionKey"].ToString()));

            List<Post> allPosts = new List<Post>();
            foreach (PostsEntity entity in table.ExecuteQuery(query))
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<PostsEntity>(entity.PartitionKey, entity.RowKey);
                TableResult retrievedResult = table.Execute(retrieveOperation);
                PostsEntity deleteEntity = (PostsEntity)retrievedResult.Result;

                if (deleteEntity != null)
                {
                    TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
                    table.Execute(deleteOperation);
                }

            }
        }

        public static bool MaximumPostsReached()
        {
            CloudTable table = GetAzureTable("posts");
            TableQuery<PostsEntity> query = new TableQuery<PostsEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ConfigurationManager.AppSettings["TablePartitionKey"].ToString()));
            return table.ExecuteQuery(query).Count() > Convert.ToInt32(ConfigurationManager.AppSettings["MaximumPosts"].ToString());
        }

        public static void SavePostToAzure(DateTime postTime, string person, string message)
        {
            if (!MaximumPostsReached())
            {
                CloudTable table = GetAzureTable("posts");
                PostsEntity postsEntity = new PostsEntity(ConfigurationManager.AppSettings["TablePartitionKey"].ToString(), Guid.NewGuid().ToString()) { PostTime = postTime, Person = person, Message = message };
                TableOperation insertOperation = TableOperation.InsertOrReplace(postsEntity);
                TableResult result = table.Execute(insertOperation);
            }
        }

    }

    public class PostsEntity : TableEntity
    {

        public PostsEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }
        public PostsEntity()
        {  }

        public DateTime PostTime { get; set; }

        public string Person { get; set; }

        public string Message { get; set; }

    }

    public class ExceptionEntity : TableEntity
    {

        public ExceptionEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }
        public ExceptionEntity()
        { }

        public DateTime ExceptionTime { get; set; }

        public string Person { get; set; }

        public string Message { get; set; }

        public string ExceptionDetails { get; set; }

    }
}