using KnowledgeBot.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
namespace KnowledgeBot.Repository;

public class CosmosDbRepository : ICosmosDbRepository
{
    private readonly Microsoft.Azure.Cosmos.Container _container;

    public CosmosDbRepository(IConfiguration configuration)
    {
        CosmosSerializationOptions options = new()
        {
            PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
        };

        CosmosClient client = new CosmosClientBuilder(configuration["CosmosDB:ConnectionString"])
                                  .WithSerializerOptions(options)
                                  .Build();

        Database db = client.GetDatabase(configuration["CosmosDB:Database"]);
        _container = db.GetContainer(configuration["CosmosDB:Container"]);
    }

    public async Task<T> InsertAsync<T>(string partitionKey, T item) where T : BaseEntity
    {
        PartitionKey key = new(partitionKey);

        return await _container.UpsertItemAsync(item, key);
    }

    public async Task<IEnumerable<T>> GetItems<T>(string query, IDictionary<string, object> parameters) where T : class
    {
        QueryDefinition queryDefinition = new QueryDefinition(query);

        foreach (var p in parameters)
        {
            queryDefinition.WithParameter(p.Key, p.Value);
        }

        FeedIterator<T> response = _container.GetItemQueryIterator<T>(query);

        List<T> entities = new List<T>();
        while (response.HasMoreResults)
        {
            FeedResponse<T> results = await response.ReadNextAsync();
            entities.AddRange(results);
        }
        return entities;
    }
}
