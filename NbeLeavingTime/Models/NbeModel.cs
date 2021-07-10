using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Caching;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace NbeLeavingTime.Models
{
    public class NbeModel
    {
 
   
        public List<Post>GetAllPosts()
        {
            var allPosts = AzureServices.GetPostsFromAzure();
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

            AzureServices.SavePostToAzure(GetAuESTime(), p.Person, p.Message);
        }

        


        public void DeleteAllPosts()
        {
            AzureServices.RemoveAllPostsFromAzure();
        }

        

        public static DateTime GetAuESTime()
        {
            TimeZoneInfo cstZone = TimeZoneInfo.FindSystemTimeZoneById("AUS Eastern Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now.ToUniversalTime(), cstZone);
        }
    }

    public class Post
    {
        public DateTime PostTime { get; set; }

        public string FormattedDate { get; set; }

        public string Person { get; set; }

        public string Message { get; set; }
    }

    public class PostSorter: IComparer<Post>
    {

        public int Compare(Post x, Post y)
        {
            return y.PostTime.CompareTo(x.PostTime);
               
        }
    }
 }