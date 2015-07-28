using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace AutoComplete.Search
{
    public static class LuceneSearch
    {
        private static string _luceneDir =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "lucene_index");
        private static FSDirectory _directoryTemp;
        private static FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(_luceneDir, "write.lock");
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }

        private static void AddToLuceneIndex(Item newItem, IndexWriter writer)
        {
            var doc = new Document();

            doc.Add(new Field("Word", newItem.Word, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field("Frequency", newItem.Frequency.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            writer.AddDocument(doc);
        }

        public static void ReadFileToLuceneIndex(string filePath)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                using (var fileReader = new StreamReader(filePath))
                {
                    string stringLine;
                    while ((stringLine = fileReader.ReadLine()) != null)
                    {
                        var splitedLine = stringLine.Split(' ');
                        if (splitedLine.Length != 2)
                        {
                            continue;
                        }
                        int freq;
                        if (!int.TryParse(splitedLine[1], out freq))
                        {
                            continue;
                        }
                        var item = new Item(splitedLine[0], freq);
                        AddToLuceneIndex(item, writer);
                    }
                }

                // close handles
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }

        private static Query parseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());                
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        public static IEnumerable<string> Search(string wordForAutocomplete)
        {
            // validation
            if (string.IsNullOrEmpty(wordForAutocomplete)) return new List<string>();
            wordForAutocomplete += '*';
            // set up lucene searcher
            using (var searcher = new IndexSearcher(_directory))
            {
                var hits_limit = 10;
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);

                var parser = new QueryParser(Version.LUCENE_30, "Word", analyzer);
                var query = parseQuery(wordForAutocomplete, parser);
                var sort = new Sort(new SortField("Frequency",4, true));
                var filter = new QueryWrapperFilter(query);
                var hits = searcher.Search(query,filter, hits_limit, sort).ScoreDocs;
                var results = hits.Select(hit => new Item(searcher.Doc(hit.Doc)).Word).ToArray();

                analyzer.Close();
                
                return results;
            }
        }
    }
}

