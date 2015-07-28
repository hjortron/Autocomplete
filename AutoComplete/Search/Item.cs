using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Documents;

namespace AutoComplete.Search
{
    class Item
    {
        public string Word { get; set; }
        public int Frequency { get; set; }

        public Item(string word, int freq)
        {
            Word = word;
            Frequency = freq;
        }

        public Item(Document doc)
        {
            Word = doc.Get("Word");
            Frequency = Convert.ToInt32(doc.Get("Frequency"));
        }
    }
}
