namespace KnowledgeBot.Models;

public record OpenAIEmbeddingResponse(List<Data> Data);

public record Data(List<float> Embedding);
