public class AppSettings
{
      public string StorageConnection { get; set; }
      public string TableName { get; set; }
      public string PartitionKey { get; set; }
      public string SiteTitle { get; set; }
      public string Hardcodednames { get; set; }
      public string HardcodedUserTableName { get; set; }  
      public string ShowLinks { get; set; } 
}


public static class GlobalAppSettings
{
      public static AppSettings Settings { get; set; }

}