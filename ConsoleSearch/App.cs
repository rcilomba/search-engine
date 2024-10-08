using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleSearch
{
    public class App
    {
        public async Task Run()
        {
            SearchLogic mSearchLogic = new SearchLogic();
            Console.WriteLine("Console Search");

            while (true)
            {
                Console.WriteLine("enter search terms - q for quit [default: hello]");
                string input = Console.ReadLine() ?? "hello"; // Search for hello by default
                if (input.Equals("q")) break;

                var wordIds = new List<int>();
                var searchTerms = input.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                foreach (var t in searchTerms)
                {
                    int id = await mSearchLogic.GetIdOfAsync(t);
                    if (id != -1)
                    {
                        wordIds.Add(id);
                    }
                    else
                    {
                        Console.WriteLine(t + " will be ignored");
                    }
                }

                DateTime start = DateTime.Now;

                var docIds = await mSearchLogic.GetDocumentsAsync(wordIds);

                // get details for the first 10             
                var top10 = new List<int>();
                foreach (var p in docIds.Take(10))
                {
                    top10.Add(p.Key);
                }

                TimeSpan used = DateTime.Now - start;

                int idx = 0;
                var documentDetails = await mSearchLogic.GetDocumentDetailsAsync(top10);
                foreach (var doc in documentDetails)
                {
                    Console.WriteLine("" + (idx + 1) + ": " + doc + " -- contains " + docIds[docIds.Keys.ToArray()[idx]] + " search terms");
                    idx++;
                }
                Console.WriteLine("Documents: " + docIds.Count + ". Time: " + used.TotalMilliseconds);

                Thread.Sleep(1000);
            }
        }
    }
}
