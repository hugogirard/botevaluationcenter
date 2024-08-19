
namespace KnowledgeBot.RAG.Implementation
{
    public class HotelService : IRetrievalService
    {
        private readonly ILogger<HotelService> _logger;

        public HotelService(ILogger<HotelService> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<string>> GetAnswersAsync(string question)
        {
            var list = new List<string>
            {
                "The hotel is located in the city center",
            };

            return await Task.FromResult(list);
        }
    }
}
