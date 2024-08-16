using KnowledgeBot.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;

namespace KnowledgeBot.Repository;

public class CosmosDbRepository : ICosmosDbRepository
{
    private readonly Dictionary<string, Microsoft.Azure.Cosmos.Container> _containers;

    public CosmosDbRepository(IConfiguration configuration, string database, List<string> containers)
    {
        CosmosSerializationOptions options = new()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        };

        CosmosClient client = new CosmosClientBuilder(configuration["CosmosDB:ConnectionString"])
                                  .WithSerializerOptions(options)
                                  .Build();

        Database db = client.GetDatabase(configuration["CosmosDB:Database"]);
        _containers = new();
        foreach (var containerName in containers)
        {
            if (!_containers.ContainsKey(containerName))
                _containers.Add(containerName, db.GetContainer(containerName));
        }
    }

    public async Task<T> InsertAsync<T>(string partitionKey, T item, string containerName) where T : BaseEntity
    {
        var container = _containers[containerName];
        PartitionKey key = new(partitionKey);

        return await container.CreateItemAsync(item);
    }

    public async Task<IEnumerable<T>> GetItems<T>(string containerName, string query, IDictionary<string, object> parameters) where T : class
    {
        var container = _containers[containerName];
        QueryDefinition queryDefinition = new QueryDefinition(query);

        foreach (var p in parameters)
        {
            queryDefinition.WithParameter(p.Key, p.Value);
        }

        FeedIterator<T> response = container.GetItemQueryIterator<T>(query);

        List<T> entities = new List<T>();
        while (response.HasMoreResults)
        {
            FeedResponse<T> results = await response.ReadNextAsync();
            entities.AddRange(results);
        }
        return entities;
    }
}
