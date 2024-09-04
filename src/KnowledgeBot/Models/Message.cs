using System;

namespace KnowledgeBot.Models
{
    public class Message : BaseEntity
    {
        public string Type { get; } = "Message";

        public DateTime TimeStamp { get; set; }

        public string Prompt { get; set; }

        public string Completion { get; set; }

        public string SessionId { get; set; }

        public bool FoundInKnowledgeDatabase { get; set; }

        public string KnowledgeBaseName { get; set; }

        public bool FoundInRetrieval { get; set; }

        public bool QuestionAnswered { get; set; } = false;

        public string RetrievalPluginName { get; set; }

        /// <summary>
        /// Partion Key in CosmosDB
        /// </summary>
        public string MemberId { get; set; }
        public bool QuestionFeedbackFromUser { get; set; }

        public Message()
        {
            Id = Guid.NewGuid().ToString();
            TimeStamp = DateTime.UtcNow;
        }
    }
}
