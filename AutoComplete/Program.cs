using System;
using System.IO;
using AutoComplete.Search;

namespace AutoComplete
{
    class Program
    {
        static void Main(string[] args)
        {
            var dictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.in");
            var fileStream = new FileStream(dictionaryPath, FileMode.Open, FileAccess.Read);
            var luceneSearch = new LuceneSearch();
            luceneSearch.LoadStreamIntoLuceneIndex(fileStream);
            var search = Console.ReadLine();
            var result = luceneSearch.Search(search);

            foreach (var word in result)
            {
                Console.WriteLine(word);               
            }
            Console.ReadKey();
        }
    }
}
