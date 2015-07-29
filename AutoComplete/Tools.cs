using AutoComplete.Properties;
using AutoComplete.Search;

namespace AutoComplete
{
    public class Tools
    {
        private static readonly Settings Settings = Settings.Default;

        public static Item ParseItemFromString(string strItem)
        {
            var splitedLine = strItem.Split(' ');
            int freq;

            if (splitedLine.Length == 2 && int.TryParse(splitedLine[Settings.FREQ_POS], out freq))
            {
                return new Item(splitedLine[Settings.WORD_POS], freq);
            }
            return null;
        }
    }
}
