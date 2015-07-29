using System;
using System.IO;
using System.Linq;
using System.Text;
using AutoComplete.Properties;
using AutoComplete.Search;
using Lucene.Net.Store;
using NUnit.Framework;

namespace AutoComplete.Tests
{
    [TestFixture]
    public class LuceneSearchTests
    {      
        [TestCase("test 2", true)]
        [TestCase("test2", false)]
        public void LoadStreamIntoLuceneIndex_ReturnsPredictableResult(string testString, bool result)
        {
            var directory = new RAMDirectory();
            var luseneSearch = new LuceneSearch(directory);
            var testStream = new MemoryStream(Encoding.UTF8.GetBytes(testString));
            Assert.AreEqual(luseneSearch.LoadStreamIntoLuceneIndex(testStream), result);
        }

        [TestCase("test")]
        public void Search_ReturnsExpectedValue(string testWord)
        {
            var directory = new RAMDirectory();
            var luseneSearch = new LuceneSearch(directory);

            var testStream = new MemoryStream(Encoding.UTF8.GetBytes(String.Format("{0} 2", testWord)));
            luseneSearch.LoadStreamIntoLuceneIndex(testStream);
            Assert.AreEqual(luseneSearch.Search(testWord.Substring(0,2)).FirstOrDefault(), testWord);            
        }

        [Test]
        public void Search_ReturnsExpectedSortedArray()
        {
            var directory = new RAMDirectory();
            var luseneSearch = new LuceneSearch(directory);
            var testWordsArray = new[] {"karetachi 4", "kanope 5", "kare 2", "kovea 1"};
            var expectedArray = new[] {"kanope", "karetachi", "kare"};
            var joinedString = String.Join(Environment.NewLine, testWordsArray);
            var testStream = new MemoryStream(Encoding.UTF8.GetBytes(joinedString));
            luseneSearch.LoadStreamIntoLuceneIndex(testStream);
            var searchResult = luseneSearch.Search("ka");
            Assert.AreEqual(searchResult, expectedArray);
        }
    }
}
