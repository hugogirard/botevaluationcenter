namespace KnowledgeBot.Models
{
    public class Session
    {
        public string Id { get; set; }

        public string Type { get; set; }

        public string SessionId { get; set; }
        
        /// <summary>
        /// Partion Key in CosmosDB
        /// </summary>
        public string MemberId { get; set; }

        public string Name { get; set; }
    }
}
