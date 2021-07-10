using Microsoft.Azure.Cosmos.Table;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

    public class AzureServices
    {

        private readonly AppSettings _appSettings;
        public AzureServices(AppSettings appSettings)
        {
            _appSettings =appSettings;
        }

        public  CloudTable GetAzureTable(string TableName)
        {
            // Retrieve the storage account from the connection string.
            string storageConnectionString = GlobalAppSettings.Settings.StorageConnection;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);


            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the table if it doesn't exist.
            CloudTable table = tableClient.GetTableReference(TableName);
            table.CreateIfNotExists(new TableRequestOptions() { RetryPolicy = new ExponentialRetry() });

            return table;
        }

        public  void SaveExceptionToAzure(Post p, Exception xcp)
        {
            CloudTable table = GetAzureTable("Exceptions");
            ExceptionEntity exceptionsEntity = null;
            if (p != null)
                exceptionsEntity = new ExceptionEntity(GlobalAppSettings.Settings.PartitionKey, Guid.NewGuid().ToString()) { ExceptionTime = DateTime.Now, Person = p.Person, Message = p.Message, ExceptionDetails = xcp.ToString() };
            else
                exceptionsEntity = new ExceptionEntity(GlobalAppSettings.Settings.PartitionKey, Guid.NewGuid().ToString()) { ExceptionTime = DateTime.Now, Person = "unknown", Message = "unknown", ExceptionDetails = xcp.ToString() };

            TableOperation insertOperation = TableOperation.InsertOrReplace(exceptionsEntity);
            TableResult result = table.Execute(insertOperation);
        }

        public  List<Post> GetPostsFromAzure()
        {
            CloudTable table = GetAzureTable(_appSettings.TableName);
            TableQuery<PostsEntity> query = new TableQuery<PostsEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,_appSettings.PartitionKey));

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

        public  PersonEntity GetPersonFromAzure(string PersonID)
        {
            CloudTable table = GetAzureTable(_appSettings.HardcodedUserTableName);
            var filter =
                TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition("PartitionKey",QueryComparisons.Equal, _appSettings.PartitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, PersonID));



            TableQuery<PersonEntity> query = new TableQuery<PersonEntity>().Where(filter);

            var queryResult=table.ExecuteQuery(query);

            return queryResult.Count()>0?queryResult.First():new PersonEntity(){PersonName="",RowKey="notfound"};

        }

      

        public  void RemoveAllPostsFromAzure()
        {
            CloudTable table = GetAzureTable(_appSettings.TableName);
            TableQuery<PostsEntity> query = new TableQuery<PostsEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _appSettings.PartitionKey));

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

        public  bool MaximumPostsReached()
        {
            CloudTable table = GetAzureTable(_appSettings.TableName);
            TableQuery<PostsEntity> query = new TableQuery<PostsEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, _appSettings.PartitionKey));
            return table.ExecuteQuery(query).Count() > Convert.ToInt32(1000);
        }

        public  void SavePostToAzure(DateTime postTime, string person, string message)
        {
            if (!MaximumPostsReached())
            {
                CloudTable table = GetAzureTable(_appSettings.TableName);
                PostsEntity postsEntity = new PostsEntity(_appSettings.PartitionKey, Guid.NewGuid().ToString()) { PostTime = postTime, Person = person, Message = message };
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

    public class PersonEntity : TableEntity
    {

        public PersonEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }
        public PersonEntity()
        {  }

        public string PersonName { get; set; }

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