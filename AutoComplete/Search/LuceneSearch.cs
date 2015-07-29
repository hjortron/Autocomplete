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
        private readonly Settings Settings;
        private string _luceneDirPath;
        private Lucene.Net.Store.Directory _directory;
        private Sort _sorter;
        private StandardAnalyzer _analyzer;
        private QueryParser _queryParser;

        public LuceneSearch()
        {
            Settings = AutoComplete.Properties.Settings.Default;
            InitComponents();
            
            var dictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.DICTIONARY_FILE_NAME);
            var fileStream = new FileStream(dictionaryPath, FileMode.Open, FileAccess.Read);

            LoadStreamIntoLuceneIndex(fileStream);                                       
        }

        public LuceneSearch(Lucene.Net.Store.Directory directory)
        {
            _directory = directory;
            Settings = Settings.Default;
            InitComponents();
        }

        private void InitComponents()
        {
            _analyzer = new StandardAnalyzer(Settings.LUCENE_VERSION);
            _queryParser = new QueryParser(Settings.LUCENE_VERSION, Settings.WORD_FIELD_NAME, _analyzer);
            _luceneDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.LUCENE_INDEX_DIRECTORY);
        }

        private Sort Sorter
        {
            get
            {
                if (_sorter == null)
                {
                   var sortField = new SortField(Settings.FREQ_FIELD_NAME, Settings.LUCENE_INTEGER_TYPE, true);
                   _sorter = new Sort(sortField);                    
                }
                return _sorter;
            }
        }

        private Lucene.Net.Store.Directory Directory
        {
            get
            {
                if (_directory == null)
                {
                    _directory = FSDirectory.Open(new DirectoryInfo(_luceneDirPath));
                }
                if (IndexWriter.IsLocked(_directory))
                {
                    IndexWriter.Unlock(_directory);
                }
                var lockFilePath = Path.Combine(_luceneDirPath, "write.lock");
                if (File.Exists(lockFilePath))
                {
                    File.Delete(lockFilePath);
                }

                return _directory;
            }
        }

        private void AddToLuceneIndex(Item newItem, IndexWriter writer)
        {
            var searchQuery = new TermQuery(new Term(Settings.WORD_FIELD_NAME, newItem.Word));
            writer.DeleteDocuments(searchQuery);

            var doc = new Document();

            doc.Add(new Field(Settings.WORD_FIELD_NAME, newItem.Word, Field.Store.YES, Field.Index.ANALYZED));
            doc.Add(new Field(Settings.FREQ_FIELD_NAME, newItem.Frequency.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));

            writer.AddDocument(doc);
        }

        public bool LoadStreamIntoLuceneIndex(Stream fileStream)
        {
            var resultState = false;
            using (var writer = new IndexWriter(Directory, _analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                using (var fileReader = new StreamReader(fileStream))
                {                    
                    while (!fileReader.EndOfStream)
                    {
                        var item = Tools.ParseItemFromString(fileReader.ReadLine());
                        if (item != null)
                        {
                            AddToLuceneIndex(item, writer);
                            resultState = true;
                        }                                                                      
                    }                  
                }
              
                writer.Optimize();             
            }

            return resultState;
        }

        private Query ParseQuery(string searchQuery, QueryParser parser)
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

        public IEnumerable<string> Search(string wordForAutocomplete)
        {           
            if (string.IsNullOrEmpty(wordForAutocomplete)) return new List<string>();
            wordForAutocomplete += '*';
           
            using (var searcher = new IndexSearcher(Directory))
            {                              
                var query = ParseQuery(wordForAutocomplete, _queryParser);
                var filter = new QueryWrapperFilter(query);
                var hits = searcher.Search(query, filter, Settings.HITS_LIMIT, Sorter).ScoreDocs;
                var results = hits.Select(hit => searcher.Doc(hit.Doc).GetField(Settings.WORD_FIELD_NAME).StringValue).ToArray();             
                
                return results;
            }
        }
    }
}

