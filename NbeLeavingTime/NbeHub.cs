using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using NbeLeavingTime.Models;
using Newtonsoft.Json;


namespace NbeLeavingTime
{
    public class NbeHub : Hub
    {
        public void GetAllPosts()
        {
            try
            {
                NbeModel m = new NbeModel();
                Clients.Caller.newPost(JsonConvert.SerializeObject(m.GetAllPosts()));
            }
            catch (Exception xcp)
            {
                AzureServices.SaveExceptionToAzure(null, xcp);
                NotifyClientsAboutError();
            }
        }


        public void AddPost(string post)
        {
            Post p=null;
            try
            {
                NbeModel m = new NbeModel();
                p = JsonConvert.DeserializeObject<Post>(post);
                if (p.Message == "clear2101")
                {
                    p.Person = "System";
                    p.Message = "Old messages cleared. Ready for new posts....";
                    m.DeleteAllPosts();
                }

                m.AddPost(p);
                Clients.Caller.postSuccessful();
                Clients.All.newPost(JsonConvert.SerializeObject(m.GetAllPosts()));
            }
            catch (Exception xcp)
            {
                AzureServices.SaveExceptionToAzure(p, xcp);
                NotifyClientsAboutError();
            }
        }

        private void NotifyClientsAboutError()
        {
            List<Post> m = new List<Post>
                {
                    new Post()
                    {
                        FormattedDate = "XXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                        Message = "Oops, Something is not working. Mark will fix it when he can get time.",
                        Person = "System",
                        PostTime = NbeModel.GetAuESTime()
                    }
                };

            Clients.Caller.postSuccessful();
            Clients.All.newPost(JsonConvert.SerializeObject(m));
        }

        

     


    }


}