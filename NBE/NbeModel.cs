using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

public class NbeModel
    {
        AzureServices _azureServices;
        private readonly AppSettings _appSettings;
        public NbeModel(AppSettings appSettings)
        {
            _appSettings =appSettings;
             _azureServices = new AzureServices(appSettings);

        }
        public List<Post>GetAllPosts()
        {
            var allPosts = _azureServices.GetPostsFromAzure();
            allPosts.Sort(new PostSorter());
            return allPosts;
        }

        public void AddPost(Post p)
        {
            if (p.Person.Length > 100)
                p.Person = p.Person.Substring(0, 99);

            if (p.Message.Length > 2000)
                p.Message = p.Message.Substring(0, 1999) + ".... (message was shortened as it was over 2000 characters)";

            p.Message = p.Message.Replace('<','.').Replace('>','.');


             if(_appSettings.Hardcodednames=="true")
             {
                PersonEntity person = _azureServices.GetPersonFromAzure(p.token);
                if(person.RowKey!="notfound" && p.token==person.RowKey)
                     _azureServices.SavePostToAzure(GetAuESTime(), p.Person, p.Message);
             }
             else
                _azureServices.SavePostToAzure(GetAuESTime(), p.Person, p.Message);
        }

        


        public void DeleteAllPosts()
        {
            _azureServices.RemoveAllPostsFromAzure();
        }

        

        public static DateTime GetAuESTime()
        {
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), cstZone);
        }
    }

  

    public class PostSorter: IComparer<Post>
    {

        public int Compare(Post x, Post y)
        {
            return y.PostTime.CompareTo(x.PostTime);
               
        }
    }