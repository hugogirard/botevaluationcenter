
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using KnowledgeBot.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace KnowledgeBot.RAG.Implementation
{
    public class HotelService : IRetrievalService
    {
        private readonly ILogger<HotelService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly SearchClient _searchClient;

        public HotelService(ILogger<HotelService> logger, 
                            IConfiguration configuration,
                            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _searchClient = new SearchClient(new Uri(_configuration["AzureSearch:Endpoint"]),
                                             _configuration["AzureSearch:IndexName"],
                                             new AzureKeyCredential(_configuration["AzureSearch:ApiKey"]));
        }

        public async Task<IEnumerable<string>> GetAnswersAsync(string question)
        {
            float[] embeddings = await GetEmbeddingAsync(question);

            if (embeddings == null)
            {
                return new List<string>();
            }

            // Do the research in the Index
            try
            {
                SearchResults<Hotel> response = await _searchClient.SearchAsync<Hotel>(question, new SearchOptions
                {
                    VectorSearch = new()
                    {
                        Queries = { new VectorizedQuery(embeddings) {
                        KNearestNeighborsCount = 3,
                        Fields = { "DescriptionVector" } } },
                    }                    
                });

                var sb = new StringBuilder();
                await foreach (SearchResult<Hotel> result in response.GetResultsAsync())
                {
                    Hotel doc = result.Document;
                    sb.AppendLine(doc.Description);
                }

                if (sb.Length > 0)
                {
                    return new List<string> { sb.ToString() };
                }
            }
            catch (Exception)
            {

                
            }




            return new List<string>();
        }

        private async Task<float[]> GetEmbeddingAsync(string question)
        {
            string endpoint = _configuration["AzureOpenAI:Endpoint"];
            string key = _configuration["AzureOpenAI:ApiKey"];
            string version = _configuration["AzureOpenAI:ApiVersion"];
            string model = _configuration["AzureOpenAI:EmbeddingModel"];

            string url = $"{endpoint}/openai/deployments/{model}/embeddings?api-version={version}";

            var http = _httpClientFactory.CreateClient();

            http.DefaultRequestHeaders.Add("api-key", key);
            
            var requestBody = new { input = question };
            
            var response = await http.PostAsJsonAsync(url, requestBody);

            if (response.IsSuccessStatusCode) 
            {
                var embeddingResponse = await response.Content.ReadFromJsonAsync<OpenAIEmbeddingResponse>();
                return embeddingResponse.Data.First().Embedding.ToArray();
               //var serializedObject = await response.Content.ReadAsStringAsync();
               //var jsonDocument = JsonDocument.Parse(serializedObject);
               //var embeddings = jsonDocument.RootElement.GetProperty("data").GetProperty("embeddings").Deserialize<float[]>();
               //return embeddings;
            }

            return null;
        }
    }
}
