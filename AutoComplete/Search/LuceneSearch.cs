using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoComplete.Properties;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;

namespace AutoComplete.Search
{
    public class LuceneSearch
    {
        private static readonly string LuceneDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.DICTIONARY_FILE_NAME);
        private static FSDirectory _directoryTemp;
        private static readonly StandardAnalyzer Analyzer = new StandardAnalyzer(Settings.Default.LUCENE_VERSION);
        private static FSDirectory Directory
        {
            get
            {
                if (_directoryTemp == null)
                {
                    _directoryTemp = FSDirectory.Open(new DirectoryInfo(LuceneDir));
                }
                if (IndexWriter.IsLocked(_directoryTemp))
                {
                    IndexWriter.Unlock(_directoryTemp);
                }
                var lockFilePath = Path.Combine(LuceneDir, "write.lock");
                if (File.Exists(lockFilePath))
                {
                    File.Delete(lockFilePath);
                }

                return _directoryTemp;
            }
        }

        private static void AddToLuceneIndex(Item newItem, IndexWriter writer)
        {
            var doc = new Document();

            doc.Add(new Field(Settings.Default.WORD_FIELD_NAME, newItem.Word, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(Settings.Default.FREQ_FIELD_NAME, newItem.Frequency.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            writer.AddDocument(doc);
        }

        public static void ReadFileToLuceneIndex(string filePath)
        {                     
            using (var writer = new IndexWriter(Directory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                using (var fileReader = new StreamReader(filePath))
                {
                    string stringLine;
                    while ((stringLine = fileReader.ReadLine()) != null)
                    {
                        var splitedLine = stringLine.Split(' ');
                        int freq;

                        if (splitedLine.Length == 2 && int.TryParse(splitedLine[Settings.Default.FREQ_POS], out freq))
                        {
                            var item = new Item(splitedLine[Settings.Default.WORD_POS], freq);
                            AddToLuceneIndex(item, writer);
                        }                                                                      
                    }
                }
              
                writer.Optimize();             
            }
        }

        private static Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());                
            }
            catch (ParseException)
            {
                //!!
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        public static IEnumerable<string> Search(string wordForAutocomplete)
        {           
            if (string.IsNullOrEmpty(wordForAutocomplete)) return new List<string>();
            wordForAutocomplete += '*';
           
            using (var searcher = new IndexSearcher(Directory))
            {                
                var parser = new QueryParser(Settings.Default.LUCENE_VERSION, Settings.Default.WORD_FIELD_NAME, Analyzer);
                var query = ParseQuery(wordForAutocomplete, parser);
                var sortField = new SortField(Settings.Default.FREQ_FIELD_NAME, Settings.Default.LUCENE_INTEGER_TYPE, true);
                var sort = new Sort(sortField);
                var filter = new QueryWrapperFilter(query);
                var hits = searcher.Search(query, filter, Settings.Default.HITS_LIMIT, sort).ScoreDocs;
                var results = hits.Select(hit => searcher.Doc(hit.Doc).GetField(Settings.Default.WORD_FIELD_NAME).StringValue).ToArray();             
                
                return results;
            }
        }
    }
}

