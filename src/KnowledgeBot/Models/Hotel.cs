using Newtonsoft.Json;

namespace KnowledgeBot.Models;

public class Hotel
{
    [JsonProperty("HotelName")]
    public string Name { get; set; }
    
    public string Description { get; set; }

    public string Category { get; set; }
    
}
