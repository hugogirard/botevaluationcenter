namespace KnowledgeBot.Models
{
    public class Session : BaseEntity
    {
        public string Type { get; } = "Session";
        
        /// <summary>
        /// Partion Key in CosmosDB
        /// </summary>
        public string MemberId { get; set; }

        public string Name { get; set; }
        public string ConversationId { get; set; }
    }
}
