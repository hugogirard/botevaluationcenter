using KnowledgeBot.Models;

namespace KnowledgeBot.Repository
{
    public interface ICosmosDbRepository
    {
        Task<IEnumerable<T>> GetItems<T>(string containerName, string query, IDictionary<string, object> parameters) where T : class;
        Task<T> InsertAsync<T>(string partitionKey, T item, string containerName) where T : BaseEntity;
    }
}