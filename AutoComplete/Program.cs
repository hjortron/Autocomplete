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
            var luceneSearchManager = new LuceneSearch();

            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
                var search = Console.ReadLine();
                var result = luceneSearchManager.Search(search);

                foreach (var word in result)
                {
                    Console.WriteLine(word);
                }
            }
        }
    }
}
