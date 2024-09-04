using KnowledgeBot.Models;

namespace KnowledgeBot.Repository
{
    public interface ICosmosDbRepository
    {
        Task<IEnumerable<T>> GetItems<T>(string query, IDictionary<string, object> parameters) where T : class;
        Task<T> InsertAsync<T>(string partitionKey, T item) where T : BaseEntity;
    }
}