namespace KnowledgeBot.Models
{
    public class Session
    {
        public string Id { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// Partion Key in CosmosDB
        /// </summary>
        public string SessionId { get; set; }
        
    }
}
