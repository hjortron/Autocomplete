using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoComplete.Properties;
using AutoComplete.Search;
using Lucene.Net.Support;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Hosting;
using Owin;

namespace AutoComplete
{
    internal class Server
    {
        private static readonly Settings Settings = Settings.Default;
        private static IDisposable SignalR { get; set; }

        public static void StartServer()
        {
            try
            {
                SignalR = WebApp.Start(Settings.SERVER_URL);
            }
            catch (TargetInvocationException)
            {
                Console.WriteLine("Error starting server!");
            }   
        }

    }

    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR();
        }
    }

    public class AutocompleteHub : Hub
    {
        private readonly LuceneSearch _searchManager;

        public AutocompleteHub()
        {
            _searchManager = new LuceneSearch();
        }

        public void GetAutocomplete(string word)
        {
            var result = _searchManager.Search(word).ToArray();
            if (result.Any())
            {
                Clients.Caller.Response(result);
            }
        }

        public override Task OnConnected()
        {
            Console.WriteLine("Client connected: " + Context.ConnectionId);

            return base.OnConnected();
        }

        public void OnDisconnected()
        {
            Console.WriteLine("Client disconnected: " + Context.ConnectionId);
        }
    }
}
