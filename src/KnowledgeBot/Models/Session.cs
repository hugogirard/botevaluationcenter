namespace KnowledgeBot.Models
{
    public class Session : BaseEntity
    {
        public string Type { get; } = "Session";

        public string SessionId { get; set; }
        
        /// <summary>
        /// Partion Key in CosmosDB
        /// </summary>
        public string MemberId { get; set; }

        public string Name { get; set; }
    }
}
