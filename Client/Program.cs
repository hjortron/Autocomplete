using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Client.Properties;
using Microsoft.AspNet.SignalR.Client;

namespace Client
{
    class Program
    {     
        private static HubConnection Connection { get; set; }
        private static IHubProxy HubProxy { get; set; }             

        static void Main(string[] args)
        {

            ConnectAsync();
            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                var word = Console.ReadLine();
                GetAutocomplete(word);                  
            }                     
        }

        private static void ConnectAsync()
        {
            Connection = new HubConnection(Settings.Default.SERVER_URL);
            HubProxy = Connection.CreateHubProxy("AutocompleteHub");
            try
            {
                Connection.Start();             
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("Unable to connect to server: Start server before connecting clients.");                
            }           
        }

        private async static void GetAutocomplete(string word)
        {           
            await HubProxy.Invoke("GetAutocomplete", word);
            HubProxy.On<List<string>>("Response", message => message.ForEach(Console.WriteLine));            
        }
    }
}
