using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json;

    public class NbeHub : Hub
    {

        public async Task GetAllPosts()
        {
            try
            {
                NbeModel m = new NbeModel(GlobalAppSettings.Settings);
                await Clients.Caller.SendAsync("newPost",JsonConvert.SerializeObject(m.GetAllPosts()));

            }
            catch (Exception xcp)
            {
                AzureServices _azuresrv = new AzureServices(GlobalAppSettings.Settings);
                _azuresrv.SaveExceptionToAzure(null, xcp);
                await NotifyClientsAboutError();
            }
        }


        public async Task  AddPost(string post)
        {
            Post p=null;
            try
            {
                NbeModel m = new NbeModel(GlobalAppSettings.Settings);
                p = JsonConvert.DeserializeObject<Post>(post);
                if (p.Message == "clear2101")
                {
                    p.Person = "System";
                    p.Message = "Old messages cleared. Ready for new posts....";
                    m.DeleteAllPosts();
                }

                m.AddPost(p);
                await Clients.Caller.SendAsync("postSuccessful");
                await Clients.All.SendAsync("newPost",JsonConvert.SerializeObject(m.GetAllPosts()));
            }
            catch (Exception xcp)
            {
                AzureServices _azuresrv = new AzureServices(GlobalAppSettings.Settings);
                _azuresrv.SaveExceptionToAzure(p, xcp);
                await NotifyClientsAboutError();
            }
        }

        private async Task NotifyClientsAboutError()
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

            await Clients.Caller.SendAsync("postSuccessful");
            await Clients.All.SendAsync("newPost",JsonConvert.SerializeObject(m));
        }
    }