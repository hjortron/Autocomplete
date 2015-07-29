using System;
using System.IO;
using System.Threading.Tasks;
using AutoComplete.Search;

namespace AutoComplete
{
    class Program
    {
      
        static void Main(string[] args)
        {          
            Task.Run(() => Server.StartServer());
            Console.ReadKey();
        }
    }
}
