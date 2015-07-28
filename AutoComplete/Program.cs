using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoComplete.Search;

namespace AutoComplete
{
    class Program
    {
        static void Main(string[] args)
        {
            var dictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.in");
            LuceneSearch.ReadFileToLuceneIndex(dictionaryPath);
            var search = Console.ReadLine();
            var result = LuceneSearch.Search(search);

            foreach (var word in result)
            {
                Console.WriteLine(word);
               
            }
            Console.ReadKey();
        }
    }
}
