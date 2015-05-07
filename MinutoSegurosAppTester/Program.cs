using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Xml.Linq;
using MinutoSegurosApp;

namespace MinutoSegurosAppTester
{
    class Program
    {
        private int _tryConnectionLimit = 4;

        static void Main(string[] args)
        {
            try
            {
                var url = @"http://www.minutoseguros.com.br/blog/feed/";

                Console.WriteLine("Reading feeds from url {0}...", url);
                Console.WriteLine();

                var rssReader = new FeedReader(5);
                rssReader.LoadRss(url);

                var list10TopWords = rssReader.TopWords(10);

                if (list10TopWords == null || list10TopWords.Count == 0)
                {
                    Console.WriteLine("The feed is empty.");
                }
                else
                {
                    Console.WriteLine("Top 10 words of feed:");

                    var count = 0;
                    foreach (var keyParValue in list10TopWords)
                    {
                        Console.WriteLine("{0:00} º => {1},{2:##,###}", ++count, keyParValue.Key.PadRight(20, '.'), keyParValue.Value);
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Geting feed's words count...");

                var lFeeds = rssReader.GetWordsCountForFeed();
                if (lFeeds.Any())
                {
                    var orderedQtdWordsFeed = lFeeds.OrderByDescending(feed => feed.Value);
                    foreach (var keyValuePair in lFeeds)
                    {
                        Console.WriteLine("\nFeed: {0}", keyValuePair.Key.PadRight(50));

                        var contents = keyValuePair.Value;
                        foreach (var contentKeyValuePair in contents)
                        {
                            Console.WriteLine("Words in {0}: {1}", contentKeyValuePair.Key.PadRight(10), contentKeyValuePair.Value);
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                Console.WriteLine("Server is out of service!\n {0}", ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine("Press any key to finish...");
                Console.ReadLine();
            }
        }
    }
}
