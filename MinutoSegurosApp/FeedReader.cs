using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MinutoSegurosApp
{
    public class FeedReader
    {
        private int _tryConnectionLimit;
        private int _triedConnections = 1;
        private IList<Feed> _feeds;
        private Dictionary<string, int> _allWords;
        private Dictionary<string, int> _topWords;
        private Dictionary<Feed, int> _topMostFeed;

        private List<string> _allExceptWords = new List<string>
        {
            "a",
            "e",
            "o",
            "ante",
            "após",
            "até",
            "com",
            "conforme",
            "contra",
            "consoante",
            "de",
            "desde",
            "durante",
            "em",
            "excepto",
            "entre",
            "mediante",
            "para",
            "perante",
            "por",
            "salvo",
            "sem",
            "segundo",
            "sob",
            "sobre",
            "trás",
            "meu",
            "seu",
            "seus",
            "sua",
            "suas",
            "nosso",
            "nossos",
            "nossa",
            "nossas",
            "teu",
            "teus",
            "tua",
            "tuas",
            "os",
            "as",
            "um",
            "uma",
            "uns",
            "umas",
            "ao",
            "à",
            "aos",
            "às",
            "de",
            "do",
            "da",
            "dos",
            "das",
            "dum",
            "duma",
            "duns",
            "dumas",
            "em",
            "no",
            "na",
            "nos",
            "nas",
            "num",
            "numa",
            "nuns",
            "numas",
            "por",
            "pelo",
            "pela",
            "pelos",
            "pelas",
            "http",
            "href",
            "nofollow",
            "post",
            "posts",
            "comments",
            "blog",
            "Minuto",
            "related",
            "first",
            "rss",
            "appeared",
            "blogminuto",
            "azurewebsites",
            "wordpress",
            "org",
            "hourly",
            "net",
            "minuto",
            "seguros",
            "the",
            "rel",
            "class",
            "yarpp-related-rss",
            "yarpp-related-none",
            "img"
        };

        public FeedReader(int tryConnectionLimit = 0)
        {
            _tryConnectionLimit = tryConnectionLimit;
        }

        public void LoadRss(string url)
        {
            XDocument feedXml = null;
            try
            {
                feedXml = XDocument.Load(url);
            }
            catch (Exception ex)
            {
                if (_triedConnections <= _tryConnectionLimit)
                {
                    _triedConnections++;
                    LoadRss(url);
                }
                else
                {
                    throw new System.Net.WebException("Failure an attempt to connect after " + _triedConnections + " attempts.", ex);
                }
            }

            if (feedXml == null)
            {
                throw new InvalidOperationException("Url does not return any data!");
            }

            var feeds = from feed in feedXml.Descendants("item")
                        select new Feed
                        {
                            Title = feed.Element("title"),
                            //Link = feed.Element("link"),
                            //Comments = feed.Element("comments"),
                            //Date = feed.Element("pubDate"),
                            //Creator = feed.Element("creator"),
                            //Category = feed.Element("category"),
                            //Guid = feed.Element("guid"),
                            Description = feed.Element("description"),
                            Content = feed.Element("content"),
                        };

            var enumerable = feeds as Feed[] ?? feeds.ToArray();
            if (enumerable.Any())
                _feeds = enumerable.ToList();

            // Count words for feed's content
            _feeds.ToList().ForEach(
                f =>
                {
                    if (f.Content != null)
                    {
                        f.ContentWordsCount = 0;
                        var array = f.Content.Value.Split(' ');
                        array.ToList().ForEach(a => f.ContentWordsCount += _allExceptWords.Contains(a) ? 0 : 1);
                    }

                    if (f.Description != null)
                    {
                        f.DescriptionWordsCount = 0;
                        var array = f.Description.Value.Split(' ');
                        array.ToList().ForEach(a => f.DescriptionWordsCount += _allExceptWords.Contains(a) ? 0 : 1);
                    }
                });
        }

        public IDictionary<string, int> TopWords(int maxResults)
        {
            if (_allWords == null)
            {
                const string pattern = @"\w(?<!\d)[\w'-]*(?!\>)";
                //const string pattern = @"\w+(?!\>)";

                _allWords = new Dictionary<string, int>();
                _topMostFeed = new Dictionary<Feed, int>();
                var text = "";
                MatchCollection matches;
                foreach (Feed feed in _feeds)
                {
                    if (feed.Description != null)
                    {
                        text = Regex.Replace(feed.Description.Value, "<.*?>", "");
                        matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                        CountWords(matches);
                    }

                    if (feed.Content != null)
                    {
                        text = Regex.Replace(feed.Content.Value, "<.*?>", "");
                        matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                        CountWords(matches);
                    }
                }
            }

            // Get 10 top words
            var listTopWords = new Dictionary<string, int>();
            var top10Words = from d in _allWords.OrderByDescending(e => e.Value)
                             select d;

            var countLimit = maxResults;
            foreach (KeyValuePair<string, int> keyValuePair in top10Words)
            {
                listTopWords.Add(keyValuePair.Key, keyValuePair.Value);
                countLimit--;
                if (countLimit <= 0)
                    break;
            }

            return listTopWords;
        }

        private void CountWords(MatchCollection words)
        {
            foreach (Match match in words)
            {
                var word = match.Value.ToLower();

                // Check exceptions words...
                if (_allExceptWords.Contains(word))
                    continue;

                if (match.Value.Length <= 4)
                    continue;

                /* Case word exists in dictionary get the value and increment
                 * 1 founded word. */
                if (_allWords.ContainsKey(word))
                    _allWords[word] = _allWords[word] + 1;
                else
                    _allWords.Add(word, 1);
            }
        }

        public Dictionary<string, Dictionary<string, int>> GetWordsCountForFeed()
        {
            return _feeds.ToDictionary(
                feed => feed.Title.Value,
                feed => new Dictionary<string, int>
                {
                    {"Content", feed.ContentWordsCount},
                    {"Description", feed.DescriptionWordsCount}
                });
        }
    }
}
