using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Indexer
{
    public class App
    {
        private readonly HttpClient _httpClient;

        public App()
        {
            _httpClient = new HttpClient { BaseAddress = new Uri("http://word-service") };
        }

        public void Run()
        {
            try
            {
                // Clear the database
                ClearDatabase();

                // Start the file indexing process
                IndexFilesInMaildir();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private void ClearDatabase()
        {
            try
            {
                _httpClient.Send(new HttpRequestMessage(HttpMethod.Delete, "Database"));
                _httpClient.Send(new HttpRequestMessage(HttpMethod.Post, "Database"));
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request failed: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while clearing the database: {ex.Message}");
            }
        }

        private void IndexFilesInMaildir()
        {
            Crawler crawler = new Crawler();
            var maildirInfo = new DirectoryInfo("maildir");

            if (!maildirInfo.Exists)
            {
                Console.WriteLine("Maildir does not exist.");
                return;
            }

            var directories = maildirInfo.GetDirectories()
                .OrderBy(d => d.Name)
                .ToList();

            DateTime start = DateTime.Now;

            foreach (var directory in directories)
            {
                crawler.IndexFilesIn(directory, new List<string> { ".txt" });
            }

            TimeSpan used = DateTime.Now - start;
            Console.WriteLine($"DONE! Used {used.TotalMilliseconds} ms");
        }
    }
}
