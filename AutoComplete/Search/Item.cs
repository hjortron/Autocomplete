using System;
using Lucene.Net.Documents;

namespace AutoComplete.Search
{

    public class Item
    {      
        public string Word { get; private set; }
        public int Frequency { get; private set; }

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
