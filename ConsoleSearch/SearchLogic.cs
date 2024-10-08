using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ConsoleSearch
{
    public class SearchLogic
    {
        private readonly HttpClient api;
        private readonly Dictionary<string, int> mWords;
        private readonly AsyncRetryPolicy<HttpResponseMessage> retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy;

        public SearchLogic()
        {
            api = new HttpClient { BaseAddress = new Uri("http://word-service") };

            // Define retry policy
            retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .RetryAsync(3); // Retry 3 times

            // Define circuit breaker policy
            circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1)); // Break after 2 failures

            mWords = GetAllWordsAsync().Result; // Fetch all words via API call
        }

        public async Task<int> GetIdOfAsync(string word)
        {
            if (mWords.ContainsKey(word))
                return mWords[word];
            return -1;
        }

        public async Task<Dictionary<int, int>> GetDocumentsAsync(List<int> wordIds)
        {
            var url = "Document/GetByWordIds?wordIds=" + string.Join("&wordIds=", wordIds);
            var response = await retryPolicy.ExecuteAsync(() => api.GetAsync(url));
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<int, int>>(content);
        }

        public async Task<List<string>> GetDocumentDetailsAsync(List<int> docIds)
        {
            var url = "Document/GetByDocIds?docIds=" + string.Join("&docIds=", docIds);
            var response = await circuitBreakerPolicy.ExecuteAsync(() => api.GetAsync(url));
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(content);
        }

        private async Task<Dictionary<string, int>> GetAllWordsAsync()
        {
            var response = await retryPolicy.ExecuteAsync(() => api.GetAsync("Word"));
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Dictionary<string, int>>(content);
        }
    }
}
